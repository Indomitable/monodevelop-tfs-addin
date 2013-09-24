//
// Microsoft.TeamFoundation.VersionControl.Client.Repository
//
// Authors:
//	Joel Reed (joelwreed@gmail.com)
//
// Copyright (C) 2007 Joel Reed
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Web.Services.Protocols;
using Microsoft.TeamFoundation.VersionControl.Common;

namespace Microsoft.TeamFoundation.VersionControl.Client
{
	[System.Web.Services.WebServiceBinding(Name="RepositorySoap", Namespace="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03")]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlIncludeAttribute(typeof(SecurityChange))]
	internal class Repository : System.Web.Services.Protocols.SoapHttpClientProtocol {
		
		private VersionControlServer versionControlServer;
		private string itemUrl;
		private string uploadUrl;

		public Repository(VersionControlServer versionControlServer, 
											Uri url, ICredentials credentials) 
			{
				this.versionControlServer = versionControlServer;
				this.Url = String.Format("{0}/{1}", url, "VersionControl/v1.0/Repository.asmx");
				this.itemUrl = String.Format("{0}/{1}", url, RepositoryConstants.DownloadUrlSuffix);
				this.uploadUrl = String.Format("{0}/{1}", url, RepositoryConstants.UploadUrlSuffix);
				this.Credentials = credentials;
			}

		public void CheckInFile(string workspaceName, string ownerName, PendingChange change)
		{
			UploadFile(workspaceName, ownerName, change, "Checkin");
		}

		public void ShelveFile(string workspaceName, string ownerName, PendingChange change)
		{
			UploadFile(workspaceName, ownerName, change, "Shelve");
		}

		private void UploadFile(string workspaceName, string ownerName, PendingChange change,
														string commandName)
		{
			FileInfo fi = new FileInfo(change.LocalItem);
			long len = fi.Length;

			UploadFile upload = new UploadFile(uploadUrl, Credentials, commandName);
			upload.AddValue(RepositoryConstants.ServerItemField, change.ServerItem);
			upload.AddValue(RepositoryConstants.WorkspaceNameField, workspaceName);
			upload.AddValue(RepositoryConstants.WorkspaceOwnerField, ownerName);
			upload.AddValue(RepositoryConstants.LengthField, len.ToString());
			upload.AddValue(RepositoryConstants.HashField, Convert.ToBase64String(change.UploadHashValue));

			// send byte range
			// TODO: handle files to large to fit in a single POST
			upload.AddValue(RepositoryConstants.RangeField, 
											String.Format("bytes=0-{0}/{1}", len-1, len));

			upload.AddFile(change.LocalItem);
			
			WebResponse response;

			try
				{
					response = upload.Send();
				}
			catch (WebException ex)
				{
					response = ex.Response;
					HttpWebResponse http_response = response as HttpWebResponse;
					if (http_response == null || http_response.StatusCode != HttpStatusCode.InternalServerError)
						throw ex;
				}

			// Get the stream associated with the response.
			//Stream receiveStream = response.GetResponseStream ();
			//StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
			//Console.WriteLine (readStream.ReadToEnd ());

			response.Close();
			//readStream.Close();
		}

		protected HttpWebResponse Invoke (Message message)
		{
			message.End();
			HttpWebResponse response = GetWebResponse(message.Request) as HttpWebResponse;

			if (response == null)
				{
					throw new TeamFoundationServerException("No response from server");
				}

			if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					string msg = String.Format("TF30063: You are not authorized to access {0} ({1}).\n--> Did you supply the correct username, password, and domain?", 
																		 (new Uri(this.Url)).Host, message.MethodName);

					//StreamReader readStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
					//					Console.Error.WriteLine (readStream.ReadToEnd ());
					//readStream.Close();

					throw new TeamFoundationServerException(msg); 
				}

			if (response.StatusCode == HttpStatusCode.InternalServerError)
				{
					throw new VersionControlException(GetExceptionMessage(response));
				}

			return response;
		}

		public string GetExceptionMessage(HttpWebResponse response)
		{
			StreamReader sr = new StreamReader(response.GetResponseStream(), new UTF8Encoding (false), false);
			XmlReader reader = new XmlTextReader(sr);
			string msg = String.Empty;
			while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element && reader.Name == "faultstring")
						{
							msg = reader.ReadElementContentAsString();
							break;
						}
				}

			response.Close();
			return msg;
		}

		public int CheckIn(Workspace workspace, string[] serverItems, string comment,
											 ref SortedList<string, bool> undoneServerItems)
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "CheckIn");

			msg.Body.WriteElementString("workspaceName", workspace.Name);
			msg.Body.WriteElementString("ownerName", workspace.OwnerName);

			msg.Body.WriteStartElement("serverItems");
			foreach (string serverItem in serverItems)
				msg.Body.WriteElementString("string", serverItem);
			msg.Body.WriteEndElement();

			// write info subtree
			msg.Body.WriteStartElement("info");

			DateTime dt = new DateTime(0);
			msg.Body.WriteAttributeString("date", dt.ToString("s"));

			msg.Body.WriteAttributeString("cset", "0");
			msg.Body.WriteAttributeString("owner", workspace.OwnerName);
			msg.Body.WriteElementString("Comment", comment);
			msg.Body.WriteElementString("CheckinNote", "");
			msg.Body.WriteElementString("PolicyOverride", "");
			msg.Body.WriteElementString("CheckinOptions", "ValidateCheckinOwner");
			msg.Body.WriteEndElement();

			int cset = 0;
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);
					cset = Convert.ToInt32(results.GetAttribute("cset"));

					List<Failure> failures = new List<Failure>();

					//TODO: read <ImplicitItems />
					//<CheckInResponse xmlns="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03"><CheckInResult cset="11486"><UndoneServerItems /><ImplicitItems /></CheckInResult><failures /></CheckInResponse>
					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element)
								{
									switch (results.Name)
										{
										case "Failure":
											failures.Add(Failure.FromXml(this, results));
											break;
										case "string":
											undoneServerItems.Add(results.ReadString(), true);
											break;
										}
								}
						}

					foreach (Failure failure in failures)
						{
							versionControlServer.OnNonFatalError(workspace, failure);
						}
				}
			
			return cset; 
		}

		public void CreateTeamProjectFolder(TeamProjectFolderOptions teamProjectOptions)
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "CreateTeamProjectFolder");
			teamProjectOptions.ToXml(msg.Body, "");
			HttpWebResponse response = Invoke(msg);
			response.Close();
		}

		public Workspace CreateWorkspace(Workspace workspace) 
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "CreateWorkspace");
			workspace.ToXml(msg.Body, "workspace");

			Workspace newWorkspace;
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);
					newWorkspace = Workspace.FromXml(this, results);
				}

			return newWorkspace;
		}

		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03/DeleteShelveset", RequestNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03", ResponseNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
			public void DeleteShelveset(string shelvesetName, string ownerName) 
			{
				this.Invoke("DeleteShelveset", new object[] { shelvesetName, ownerName});
			}

		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03/DeleteWorkspace", RequestNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03", ResponseNamespace="http://schemas.microsoft.com/TeamFoundation/2005/06/VersionControl/ClientServices/03", ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped, Use=System.Web.Services.Description.SoapBindingUse.Literal)]
			public void DeleteWorkspace(string workspaceName, string ownerName) 
			{
				this.Invoke("DeleteWorkspace", new object[] { workspaceName, ownerName});
			}
	
		public LabelResult[] LabelItem(Workspace workspace, VersionControlLabel label, 
																	 LabelItemSpec[] labelSpecs, LabelChildOption children)
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "LabelItem");

			msg.Body.WriteElementString("workspaceName", workspace.Name);
			msg.Body.WriteElementString("workspaceOwner", workspace.OwnerName);
			label.ToXml(msg.Body, "label");

			msg.Body.WriteStartElement("labelSpecs");
			foreach (LabelItemSpec labelSpec in labelSpecs)
				{
					labelSpec.ToXml(msg.Body, "LabelItemSpec");
				}
			msg.Body.WriteEndElement();

			msg.Body.WriteElementString("children", children.ToString());

			List<LabelResult> labelResults = new List<LabelResult>();
			List<Failure> faillist = new List<Failure>();

			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element)
								{
									switch (results.Name)
										{
										case "LabelResult":
											labelResults.Add(LabelResult.FromXml(this, results));
											break;
										case "Failure":
											faillist.Add(Failure.FromXml(this, results));
											break;
										}
								}
						}
				}

			foreach (Failure failure in faillist)
				{
					versionControlServer.OnNonFatalError(workspace, failure);
				}

			return labelResults.ToArray();
		}

		public void Shelve(Workspace workspace, Shelveset shelveset,
											 string[] serverItems, ShelvingOptions options)
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "Shelve");

			msg.Body.WriteElementString("workspaceName", workspace.Name);
			msg.Body.WriteElementString("workspaceOwner", workspace.OwnerName);

			msg.Body.WriteStartElement("serverItems");
			foreach (string serverItem in serverItems)
				msg.Body.WriteElementString("string", serverItem);
			msg.Body.WriteEndElement();

			shelveset.ToXml(msg.Body, "shelveset");

			bool replace = (options & ShelvingOptions.Replace) == ShelvingOptions.Replace;
			msg.Body.WriteElementString("replace", replace.ToString().ToLower());

			using (HttpWebResponse response = Invoke(msg))
				{
					msg.ResponseReader(response);
				}
		}

		public LabelResult[] UnlabelItem(Workspace workspace, string labelName,
																		 string labelScope, ItemSpec[] itemSpecs, 
																		 VersionSpec version)
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "UnlabelItem");

			msg.Body.WriteElementString("workspaceName", workspace.Name);
			msg.Body.WriteElementString("workspaceOwner", workspace.OwnerName);
			msg.Body.WriteElementString("labelName", labelName);

			if (!String.IsNullOrEmpty(labelScope)) 
				msg.Body.WriteElementString("labelScope", labelScope);

			msg.Body.WriteStartElement("items");
			foreach (ItemSpec itemSpec in itemSpecs)
				{
					itemSpec.ToXml(msg.Body, "ItemSpec");
				}
			msg.Body.WriteEndElement();

			version.ToXml(msg.Body, "version");

			List<LabelResult> labelResults = new List<LabelResult>();
			List<Failure> faillist = new List<Failure>();

			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element)
								{
									switch (results.Name)
										{
										case "LabelResult":
											labelResults.Add(LabelResult.FromXml(this, results));
											break;
										case "Failure":
											faillist.Add(Failure.FromXml(this, results));
											break;
										}
								}
						}
				}

			foreach (Failure failure in faillist)
				{
					versionControlServer.OnNonFatalError(workspace, failure);
				}

			return labelResults.ToArray();
		}

		public GetOperation[] Get(string workspaceName, string ownerName, 
															GetRequest[] requests, bool force, bool noGet) {

			Message msg = new Message(GetWebRequest (new Uri(Url)), "Get");
			msg.Body.WriteElementString("workspaceName", workspaceName);
			msg.Body.WriteElementString("ownerName", ownerName);
			
			msg.Body.WriteStartElement("requests");
			foreach (GetRequest request in requests)
				{
					request.ToXml(msg.Body, "");
				}
			msg.Body.WriteEndElement();

			msg.Body.WriteElementString("force", force.ToString().ToLower());
			msg.Body.WriteElementString("noGet", noGet.ToString().ToLower());

			List<GetOperation> operations = new List<GetOperation>();
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element &&
									results.Name == "GetOperation")
								operations.Add(GetOperation.FromXml(ItemUrl, results));
						}
				}

			return operations.ToArray();
		}

		public RepositoryProperties GetRepositoryProperties() 
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "GetRepositoryProperties");
			RepositoryProperties properties;
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);
					properties = RepositoryProperties.FromXml(this, results);
				}

			return properties;
		}

		public GetOperation[] PendChanges(Workspace workspace, ChangeRequest[] changes) 
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "PendChanges");

			msg.Body.WriteElementString("workspaceName", workspace.Name);
			msg.Body.WriteElementString("ownerName", workspace.OwnerName);
			msg.Body.WriteStartElement("changes");
			foreach (ChangeRequest change in changes)
				{
					change.ToXml(msg.Body, "");
				}
			msg.Body.WriteEndElement();
			
			List<GetOperation> operations = new List<GetOperation>();
			List<Failure> faillist = new List<Failure>();

			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element)
								{
									switch (results.Name)
										{
										case "GetOperation":
											operations.Add(GetOperation.FromXml(ItemUrl, results));
											break;
										case "Failure":
											faillist.Add(Failure.FromXml(this, results));
											break;
										}
								}
						}
				}

			foreach (Failure failure in faillist)
				{
					versionControlServer.OnNonFatalError(workspace, failure);
				}

			return operations.ToArray();
		}

		public Annotation[] QueryAnnotation(string annotationName, string annotatedItem, int version)
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "QueryAnnotation");

			msg.Body.WriteElementString("annotationName", annotationName);
			msg.Body.WriteElementString("annotatedItem", annotatedItem);
			msg.Body.WriteElementString("version", Convert.ToString(version));

			List<Annotation> labels = new List<Annotation>();
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element &&
									results.Name == "Annotation")
								labels.Add(Annotation.FromXml(this, results));
						}
				}

			return labels.ToArray();
		}

		public BranchHistoryTreeItem[][] QueryBranches(string workspaceName, string workspaceOwner, 
																									 ItemSpec[] itemSpecs, VersionSpec versionSpec)
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "QueryBranches");
			
			if (!String.IsNullOrEmpty(workspaceName)) msg.Body.WriteElementString("workspaceName", workspaceName);
			if (!String.IsNullOrEmpty(workspaceOwner)) msg.Body.WriteElementString("workspaceOwner", workspaceOwner);
			msg.Body.WriteStartElement("items");
			foreach (ItemSpec itemSpec in itemSpecs)
				itemSpec.ToXml(msg.Body, "ItemSpec");
			msg.Body.WriteEndElement();

			versionSpec.ToXml(msg.Body, "version");

			List<BranchHistoryTreeItem[]> tree = new List<BranchHistoryTreeItem[]>();
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);
					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element &&
									results.Name == "ArrayOfBranchRelative")
								{
									List<BranchRelative> branches = new List<BranchRelative>();
									while (results.Read())
										{
											if (results.NodeType == XmlNodeType.EndElement &&
													results.Name == "ArrayOfBranchRelative") break;
											if (results.NodeType == XmlNodeType.Element &&
													results.Name == "BranchRelative")
												branches.Add(BranchRelative.FromXml(this, results));
										}

									if (branches.Count > 0)
										{
											List<BranchHistoryTreeItem> items = new List<BranchHistoryTreeItem>();
											items.Add(new BranchHistoryTreeItem(branches.ToArray()));
											tree.Add(items.ToArray());
										}
								}
						}
				}

			return tree.ToArray();
		}

		public Changeset QueryChangeset(int changesetId, bool includeChanges,
																		bool generateDownloadUrls)
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "QueryChangeset");

			msg.Body.WriteElementString("changesetId", Convert.ToString(changesetId));
			msg.Body.WriteElementString("includeChanges", Convert.ToString(includeChanges).ToLower());
			msg.Body.WriteElementString("generateDownloadUrls", Convert.ToString(generateDownloadUrls).ToLower());
			
			Changeset changeset = null;

			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);
					changeset = Changeset.FromXml(this, results);
				}

			return changeset;
		}

		public int QueryHistory (string workspaceName, string workspaceOwner,
														 ItemSpec itemSpec, VersionSpec version,
														 string user, VersionSpec versionFrom,
														 VersionSpec versionTo, int maxCount,
														 bool includeFiles, bool slotMode,
														 bool generateDownloadUrls, ref List<Changeset> changes)
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "QueryHistory");

			if (!String.IsNullOrEmpty(workspaceName)) msg.Body.WriteElementString("workspaceName", workspaceName);
			if (!String.IsNullOrEmpty(workspaceOwner)) msg.Body.WriteElementString("workspaceOwner", workspaceOwner);

			itemSpec.ToXml(msg.Body, "itemSpec");
			if (version != null) version.ToXml(msg.Body, "versionItem");
			if (versionFrom != null) versionFrom.ToXml(msg.Body, "versionFrom");
			if (versionTo != null) versionTo.ToXml(msg.Body, "versionTo");

			if (!String.IsNullOrEmpty(user)) msg.Body.WriteElementString("user", user);
			msg.Body.WriteElementString("maxCount", Convert.ToString(maxCount));
			msg.Body.WriteElementString("includeFiles", Convert.ToString(includeFiles).ToLower());
			msg.Body.WriteElementString("generateDownloadUrls", Convert.ToString(generateDownloadUrls).ToLower());
			msg.Body.WriteElementString("slotMode", Convert.ToString(slotMode).ToLower());

			int cnt = 0;
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element &&
									results.Name == "Changeset")
								{
									changes.Add(Changeset.FromXml(this, results));
									cnt++;
								}
						}
				}

			return cnt;
		}

		public ItemSet[] QueryItems(string workspaceName, string workspaceOwner, 
																ItemSpec[] itemSpecs, VersionSpec versionSpec, 
																DeletedState deletedState, ItemType itemType, 
																bool generateDownloadUrls) 
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "QueryItems");
			
			if (!String.IsNullOrEmpty(workspaceName)) msg.Body.WriteElementString("workspaceName", workspaceName);
			if (!String.IsNullOrEmpty(workspaceOwner)) msg.Body.WriteElementString("workspaceOwner", workspaceOwner);
			msg.Body.WriteStartElement("items");
			foreach (ItemSpec itemSpec in itemSpecs)
				itemSpec.ToXml(msg.Body, "ItemSpec");
			msg.Body.WriteEndElement();

			versionSpec.ToXml(msg.Body, "version");

			msg.Body.WriteElementString("deletedState", 
																	deletedState.ToString());
			msg.Body.WriteElementString("itemType", 
																	itemType.ToString());
			msg.Body.WriteElementString("generateDownloadUrls", 
																	generateDownloadUrls.ToString().ToLower());

			List<ItemSet> itemSet = new List<ItemSet>();
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element &&
									results.Name == "ItemSet")
								itemSet.Add(ItemSet.FromXml(this, results));
						}
				}

			return itemSet.ToArray();
		}

		public ExtendedItem[][] QueryItemsExtended(string workspaceName, string workspaceOwner, 
																							 ItemSpec[] itemSpecs,	
																							 DeletedState deletedState, ItemType itemType) 
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "QueryItemsExtended");
			
			if (!String.IsNullOrEmpty(workspaceName)) msg.Body.WriteElementString("workspaceName", workspaceName);
			if (!String.IsNullOrEmpty(workspaceOwner)) msg.Body.WriteElementString("workspaceOwner", workspaceOwner);

			msg.Body.WriteStartElement("items");
			foreach (ItemSpec itemSpec in itemSpecs)
				{
					itemSpec.ToXml(msg.Body, "ItemSpec");
				}
			msg.Body.WriteEndElement();

			msg.Body.WriteElementString("deletedState", 
																	deletedState.ToString());
			msg.Body.WriteElementString("itemType", 
																	itemType.ToString());

			List< ExtendedItem[] > listOfItemArrays = new List<ExtendedItem[] >();
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);
					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element &&
									results.Name == "ArrayOfExtendedItem")
								{
									List<ExtendedItem> items = new List<ExtendedItem>();
									while (results.Read())
										{
											//Console.WriteLine("	 " + results.Name + ":" + results.NodeType);
											if (results.NodeType == XmlNodeType.EndElement &&
													results.Name == "ArrayOfExtendedItem") break;
											if (results.NodeType == XmlNodeType.Element &&
													results.Name == "ExtendedItem")
												items.Add(ExtendedItem.FromXml(this, results));
										}
									listOfItemArrays.Add(items.ToArray());
								}
						}
				}

			return listOfItemArrays.ToArray();
		}

		public VersionControlLabel[] QueryLabels(string workspaceName, string workspaceOwner, 
																						 string labelName, string labelScope, 
																						 string owner, string filterItem, 
																						 VersionSpec versionFilterItem, 
																						 bool includeItems, bool generateDownloadUrls) 
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "QueryLabels");

			if (!String.IsNullOrEmpty(workspaceName)) msg.Body.WriteElementString("workspaceName", workspaceName);
			if (!String.IsNullOrEmpty(workspaceOwner)) msg.Body.WriteElementString("workspaceOwner", workspaceOwner);
			if (!String.IsNullOrEmpty(labelName)) msg.Body.WriteElementString("labelName", labelName);
			if (!String.IsNullOrEmpty(labelScope)) msg.Body.WriteElementString("labelScope", labelScope);
			if (!String.IsNullOrEmpty(owner)) msg.Body.WriteElementString("owner", owner);
			if (!String.IsNullOrEmpty(filterItem)) msg.Body.WriteElementString("filterItem", filterItem);

			if (null != versionFilterItem) versionFilterItem.ToXml(msg.Body, "versionFilterItem");
			msg.Body.WriteElementString("includeItems", includeItems.ToString().ToLower());
			msg.Body.WriteElementString("generateDownloadUrls", generateDownloadUrls.ToString().ToLower());

			List<VersionControlLabel> labels = new List<VersionControlLabel>();
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element &&
									results.Name == "VersionControlLabel")
								labels.Add(VersionControlLabel.FromXml(this, results));
						}
				}

			return labels.ToArray();
		}

		public Item[] QueryItemsById(int[] ids, int changeSet,
																 bool generateDownloadUrls) 
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "QueryItemsById");

			msg.Body.WriteStartElement("itemIds");
			foreach (int id in ids)
				{
					msg.Body.WriteElementString("int", Convert.ToString(id));
				}
			msg.Body.WriteEndElement();

			msg.Body.WriteElementString("changeSet", Convert.ToString(changeSet));
			msg.Body.WriteElementString("generateDownloadUrls", 
																	generateDownloadUrls.ToString().ToLower());

			List<Item> items = new List<Item>();
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element &&
									results.Name == "Item")
								items.Add(Item.FromXml(this, results));
						}
				}

			return items.ToArray();
		}

		public ItemSecurity[] QueryItemPermissions(string[] identityNames, string[] items, 
																							 RecursionType recursion) 
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "QueryItemPermissions");

			msg.Body.WriteStartElement("itemSpecs");
			foreach (string item in items)
				{
					ItemSpec spec = new ItemSpec(item, recursion);
					spec.ToXml(msg.Body, "ItemSpec");
				}
			msg.Body.WriteEndElement();

			List<ItemSecurity> itemSecurities = new List<ItemSecurity>();
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element &&
									results.Name == "ItemSecurity")
								itemSecurities.Add(ItemSecurity.FromXml(this, results));
						}
				}

			return itemSecurities.ToArray();
		}

		public ChangesetMerge[] QueryMerges(string workspaceName, string workspaceOwner, 
																				ItemSpec source, VersionSpec versionSource,
																				ItemSpec target, VersionSpec versionTarget,
																				VersionSpec versionFrom, VersionSpec versionTo,
																				int maxChangesets)
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "QueryMerges");

			if (!String.IsNullOrEmpty(workspaceName)) msg.Body.WriteElementString("workspaceName", workspaceName);
			if (!String.IsNullOrEmpty(workspaceOwner)) msg.Body.WriteElementString("workspaceOwner", workspaceOwner);

			if (source != null) source.ToXml(msg.Body, "source");
			if (versionSource != null) versionSource.ToXml(msg.Body, "versionSource");

			target.ToXml(msg.Body, "target");
			versionTarget.ToXml(msg.Body, "versionTarget");

			if (versionFrom != null) versionFrom.ToXml(msg.Body, "versionFrom");
			if (versionTo != null) versionTo.ToXml(msg.Body, "versionTo");
			
			msg.Body.WriteElementString("maxChangesets", Convert.ToString(maxChangesets));
																				
			List<ChangesetMerge> merges = new List<ChangesetMerge>();
			Dictionary<int, Changeset> changesets = new Dictionary<int, Changeset>();
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType != XmlNodeType.Element) continue;

							if (results.Name == "ChangesetMerge")
								{
									merges.Add(ChangesetMerge.FromXml(this, results));
								}
							else if (results.Name == "Changeset")
								{
									Changeset changeset = Changeset.FromXml(this, results);
									changesets.Add(changeset.ChangesetId, changeset);
								}
						}
				}

			foreach (ChangesetMerge merge in merges)
				{
					Changeset changeset;
					if (changesets.TryGetValue(merge.TargetVersion, out changeset))
							merge.TargetChangeset = changeset;
				}

			return merges.ToArray();
		}

		public PendingChange[] QueryPendingSets(string localWorkspaceName, string localWorkspaceOwner,
																						string queryWorkspaceName, string ownerName,
																						ItemSpec[] itemSpecs, bool generateDownloadUrls,
																						out Failure[] failures) 
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "QueryPendingSets");
			msg.Body.WriteElementString("localWorkspaceName", localWorkspaceName);
			msg.Body.WriteElementString("localWorkspaceOwner", localWorkspaceOwner);
			msg.Body.WriteElementString("queryWorkspaceName", queryWorkspaceName);
			msg.Body.WriteElementString("ownerName", ownerName);

			msg.Body.WriteStartElement("itemSpecs");
			foreach (ItemSpec item in itemSpecs)
				{
					item.ToXml(msg.Body, "ItemSpec");
				}
			msg.Body.WriteEndElement();

			msg.Body.WriteElementString("generateDownloadUrls", 
																	generateDownloadUrls.ToString().ToLower());

			List<PendingChange> changes = new List<PendingChange>();
			List<Failure> faillist = new List<Failure>();

			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element)
								{
									switch (results.Name)
										{
										case "PendingChange":
											changes.Add(PendingChange.FromXml(this, results));
											break;
										case "Failure":
											faillist.Add(Failure.FromXml(this, results));
											break;
										}
								}
						}
				}

			failures = faillist.ToArray();
			return changes.ToArray();
		}

		public Shelveset[] QueryShelvesets (string shelvesetName, string shelvesetOwner)
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "QueryShelvesets");
			if (!String.IsNullOrEmpty(shelvesetName)) msg.Body.WriteElementString("shelvesetName", shelvesetName);
			msg.Body.WriteElementString("ownerName", shelvesetOwner);

			List<Shelveset> shelvesets = new List<Shelveset>();
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element &&
									results.Name == "Shelveset")
								shelvesets.Add(Shelveset.FromXml(this, results));
						}
				}

			shelvesets.Sort(ShelvesetGenericComparer.Instance);
			return shelvesets.ToArray();
		}

		public Workspace QueryWorkspace(string workspaceName, string ownerName) 
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "QueryWorkspace");
			msg.Body.WriteElementString("workspaceName", workspaceName);
			msg.Body.WriteElementString("ownerName", ownerName);

			Workspace workspace;
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);
					workspace = Workspace.FromXml(this, results);
				}

			return workspace;
		}

		public Workspace[] QueryWorkspaces(string workspaceName, string ownerName,
																			 string computer) 
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "QueryWorkspaces");
			if (!String.IsNullOrEmpty(workspaceName)) msg.Body.WriteElementString("workspaceName", workspaceName);
			if (!String.IsNullOrEmpty(ownerName)) msg.Body.WriteElementString("ownerName", ownerName);
			if (!String.IsNullOrEmpty(computer)) msg.Body.WriteElementString("computer", computer);

			List<Workspace> workspaces = new List<Workspace>();
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element &&
									results.Name == "Workspace")
								workspaces.Add(Workspace.FromXml(this, results));
						}
				}

			workspaces.Sort();
			return workspaces.ToArray();
		}

		public Workspace UpdateWorkspace(string oldWorkspaceName, string ownerName, 
																		 Workspace newWorkspace) 
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "UpdateWorkspace");
			msg.Body.WriteElementString("oldWorkspaceName", oldWorkspaceName);
			msg.Body.WriteElementString("ownerName", ownerName);
			newWorkspace.ToXml(msg.Body, "newWorkspace");

			Workspace workspace;
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);
					workspace = Workspace.FromXml(this, results);
				}

			return workspace;
		}

		public GetOperation[] UndoPendingChanges(string workspaceName, string ownerName, 
																						 ItemSpec[] itemSpecs) 
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "UndoPendingChanges");
			msg.Body.WriteElementString("workspaceName", workspaceName);
			msg.Body.WriteElementString("ownerName", ownerName);

			msg.Body.WriteStartElement("items");
			foreach (ItemSpec item in itemSpecs)
				{
					item.ToXml(msg.Body, "ItemSpec");
				}
			msg.Body.WriteEndElement();

			List<GetOperation> operations = new List<GetOperation>();
			using (HttpWebResponse response = Invoke(msg))
				{
					XmlReader results = msg.ResponseReader(response);

					while (results.Read())
						{
							if (results.NodeType == XmlNodeType.Element &&
									results.Name == "GetOperation")
								operations.Add(GetOperation.FromXml(ItemUrl, results));
						}
				}

			return operations.ToArray();
		}

		public void UpdateLocalVersion(UpdateLocalVersionQueue queue)
		{
			Message msg = new Message(GetWebRequest (new Uri(Url)), "UpdateLocalVersion");
			queue.ToXml(msg.Body, "UpdateLocalVersion");

			HttpWebResponse response = Invoke(msg);
			response.Close();
		}

		public VersionControlServer VersionControlServer
		{
			get { return versionControlServer; }
		}

		public string ItemUrl
		{
			get { return itemUrl; }
		}

	}

	class TfsPath
	{
		private static bool runningOnUnix = true;
		public static StringComparison PlatformComparison = StringComparison.InvariantCulture;

		static TfsPath()
		{
			int p = (int) Environment.OSVersion.Platform;
			if (!((p == 4) || (p == 128))) runningOnUnix = false;

			if (!runningOnUnix) PlatformComparison = StringComparison.InvariantCultureIgnoreCase;
		}

		public static string ToPlatformPath(string path)
		{
			if (String.IsNullOrEmpty(path)) return path;

			// TFS servers corrupt *nix type paths
			if (!runningOnUnix) return path;
			return path.Remove(0, 2).Replace('\\', '/');
		}

		public static string FromPlatformPath(string path)
		{
			if (String.IsNullOrEmpty(path)) return path;

			// TFS servers corrupt *nix type paths
			if (!runningOnUnix) return path;
			return "C:" + path.Replace('/', '\\');
		}
	}
}

// CheckoutCommandHandler.cs
// 
// Author:
//       Ventsislav Mladenov
// 
// The MIT License (MIT)
// 
// Copyright (c) 2013-2015 Ventsislav Mladenov
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace MonoDevelop.VersionControl.TFS.Commands
{
	//	class CheckoutCommandHandler : CommandHandler
	//	{
	//		protected override void Run(object dataItem)
	//		{
	//			var fileItem = IdeApp.ProjectOperations.CurrentSelectedItem as IFileItem;
	//			var folderItem = IdeApp.ProjectOperations.CurrentSelectedItem as IFolderItem;
	//			if (fileItem == null) //Only files for now.
	//                return;
	//			var workspaceItem = IdeApp.ProjectOperations.CurrentSelectedWorkspaceItem;
	//			var tfsRepo = (TFSRepository)VersionControlService.GetRepository(workspaceItem);
	//			var path = fileItem == null ? folderItem.BaseDirectory : fileItem.FileName;
	//			tfsRepo.CheckoutFile(path);
	//		}
	//
	//		protected override void Update(CommandInfo info)
	//		{
	//			var workspaceItem = IdeApp.ProjectOperations.CurrentSelectedWorkspaceItem;
	//			var tfsRepo = VersionControlService.GetRepository(workspaceItem) as TFSRepository;
	//			if (tfsRepo == null)
	//			{
	//				info.Visible = false;
	//				return;
	//			}
	//			var fileItem = IdeApp.ProjectOperations.CurrentSelectedItem as IFileItem;
	//			var folderItem = IdeApp.ProjectOperations.CurrentSelectedItem as IFolderItem;
	//			if (fileItem == null && folderItem == null)
	//			{
	//				info.Visible = false;
	//				return;
	//			}
	//			var path = fileItem == null ? folderItem.BaseDirectory : fileItem.FileName;
	//			var versionInfo = tfsRepo.GetVersionInfo(path);
	//			if (!versionInfo.IsVersioned || versionInfo.HasLocalChanges)
	//			{
	//				info.Visible = false;
	//				return;
	//			}
	//		}
	//	}
}

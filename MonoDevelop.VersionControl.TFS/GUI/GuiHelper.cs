//
// GuiHelper.cs
//
// Author:
//       Ventsislav Mladenov <vmladenov.mladenov@gmail.com>
//
// Copyright (c) 2013 Ventsislav Mladenov
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
using System;
using Xwt;
using Microsoft.TeamFoundation.VersionControl.Client.Enums;

namespace MonoDevelop.VersionControl.TFS.GUI
{
    public static class GuiHelper
    {
        public static ComboBox GetLockLevelComboBox(bool forceLock = false)
        {
            ComboBox lockLevelBox = new ComboBox();
            lockLevelBox.WidthRequest = 150;

            if (!forceLock)
                lockLevelBox.Items.Add(CheckOutLockLevel.Unchanged, "Unchanged - Keep any existing lock.");
            lockLevelBox.Items.Add(CheckOutLockLevel.CheckOut, "Check Out - Prevent other users from checking out and checking in");
            lockLevelBox.Items.Add(CheckOutLockLevel.CheckIn, "Check In - Prevent other users from checking in but allow checking out");
            if (forceLock && TFSVersionControlService.Instance.CheckOutLockLevel == CheckOutLockLevel.Unchanged)
                lockLevelBox.SelectedItem = CheckOutLockLevel.CheckOut;
            else
                lockLevelBox.SelectedItem = TFSVersionControlService.Instance.CheckOutLockLevel;
            return lockLevelBox;
        }
    }
}
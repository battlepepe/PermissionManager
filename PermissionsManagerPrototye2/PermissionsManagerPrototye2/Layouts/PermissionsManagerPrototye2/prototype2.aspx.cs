using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.WebControls;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace PermissionsManagerPrototye2.Layouts.PermissionsManagerPrototye2
{
    public partial class prototype2 : LayoutsPageBase
    {
        private SPBasePermissions perms = SPBasePermissions.ManagePermissions;
        //private List<Label> GroupsWarningList;
        private List<userPermissions> comparingUPs;
        private TransactionCollection addTransCol;
        private TransactionCollection removeTransCol;

        #region start
        //TODO needs review
        protected void Page_Load(object sender, EventArgs e)
        {
            SPUser currUser = SPContext.Current.Web.CurrentUser;
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                SPSite mysite = SPContext.Current.Site;

                using (SPWeb myweb = mysite.OpenWeb())
                {
                    SPRoleDefinition roleDef = myweb.RoleDefinitions["Full Control"];
                    SPBasePermissions basePerms = roleDef.BasePermissions;
                    String login = currUser.LoginName;
                    if (!myweb.DoesUserHavePermissions(login, basePerms))
                    {
                        Response.Redirect("../AccessDenied.aspx");
                    }
                }
            });
            //adding javascript event handlers 
            if (!IsPostBack)
            {
                MILactionDD.Attributes.Add("onchange", "javascript:changeAction()");
                MILtypeDD.Attributes.Add("onchange", "javascript:changeObjectList()");
                MILselectItemsBtn.Attributes.Add("onclick", "javascript:changeItem()");
                MILCompareBtn.Attributes.Add("onclick", "javascript:changeItem()");
            }

            
            if (removeTransCol == null)
            {
                removeTransCol = (TransactionCollection)ViewState["removeTransCol"];
            }
            if (addTransCol == null)
            {
                addTransCol = (TransactionCollection)ViewState["addTransCol"];
            }
            if(comparingUPs == null)
            {
                comparingUPs = new List<userPermissions>();
            }
            buildRemovingTransTable(sender, e);
            buildAddingTransTable(sender, e);
            if (MILAddPermListBox.Items.Count < 1)
            {
                loadPermsTable(sender, e);
            }
            if (MILRemoveListBox.Items.Count < 1)
            {
                loadUserPermsTable(sender, e);
            }
        }
        protected void selectAction(object sender, EventArgs e)
        {
            MILRemoveListBox.Items.Clear();
            if (MILAddPermListBox.Items.Count < 1)
            {
                loadPermsTable(sender, e);
            }
            if (MILRemoveListBox.Items.Count < 1)
            {
                loadUserPermsTable(sender, e);
            }
            
          
        }

        //TODO needs review
        /// <summary>
        /// if the user selects Please Pick Another then the 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void loadTable(object sender, EventArgs e)
        {
            if (MILactionDD.Text != "PPA" && MILtypeDD.Text != "PPA")
            {
                MILCompareBtn.Style.Remove("display");
                MILCompareBtn.Style.Add("display", "none");

                MILtypeDD.Style.Remove("display");
                MILtypeDD.Style.Add("display", "block");

                MILPPicker.Style.Remove("display");
                MILPPicker.Style.Add("display", "block");

                MILobjectList.Style.Remove("display");
                MILobjectList.Style.Add("display", "block");

                MILselectItemsBtn.Style.Remove("display");
                MILselectItemsBtn.Style.Add("display", "block");
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {
                    MILRemoveListBox.Items.Clear();
                    buildTable();
                });
            }
        }

        //TODO needs review
        protected void loadPermsTable(object sender, EventArgs e)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                buildPermsTable();
            });
        }


        //TODO needs lots of review
        protected void loadUserPermsTable(object sender, EventArgs e)
        {
            MILRemoveErrorLabel.Text = "";
            if (MILPPicker.Entities.Count > 0 && MILobjectList.Items.Count > 0 && MILactionDD.Text.Equals("Removing"))
            {
                SPSecurity.RunWithElevatedPrivileges(delegate ()
                {
                    SPSite mysite = SPContext.Current.Site;

                    using (SPWeb myweb = mysite.OpenWeb())
                    {
                        try
                        {
                            MILRemoveListBox.Items.Clear();
                            if (MILPPicker.ResolvedEntities.Count > 0)
                            {
                                UserData user = getUserFromPP(MILPPicker)[0];
                                if (MILtypeDD.Text == "Lists")
                                {
                                    foreach (ListItem item in MILobjectList.Items)
                                    {
                                        if (item.Selected)
                                        {

                                            Guid id = new Guid(item.Value);
                                            SPList list = myweb.Lists[id];//if user still has item selected and switches lists it will break, need to clear list on switching type
                                            String text = list.Title;
                                            String value = "";
                                            Label label = new Label();
                                            String groupsText = "";
                                            SPPermissionInfo info = list.GetUserEffectivePermissionInfo(user.userAcct.LoginName);
                                            foreach (SPRoleAssignment role in info.RoleAssignments)
                                            {
                                                foreach (SPRoleDefinition roleDef in role.RoleDefinitionBindings)
                                                {
                                                    if (!roleDef.Name.Equals("Limited Access") && !roleDef.Name.Equals("Approve") && !roleDef.Name.Equals("Manage Hierarchy") && !roleDef.Name.Equals("Restricted Read"))
                                                    {
                                                        if (role.Member is SPGroup)
                                                        {
                                                            SPGroup group = (SPGroup)role.Member;
                                                            if (groupsText.Equals(""))
                                                            {
                                                                groupsText = "*WARNING* user is part of: ";
                                                            }
                                                            groupsText += "\"" + group.Name + "\", ";
                                                        }
                                                        else
                                                        {
                                                            SPUser user2 = (SPUser)role.Member;
                                                            if (user2.IsDomainGroup)
                                                            {
                                                                if (groupsText.Equals(""))
                                                                {
                                                                    groupsText = "*WARNING* user is part of: ";
                                                                }
                                                                groupsText += "\"" + user2.Name + "\"(Active Directory Group), ";
                                                            }
                                                            else
                                                            {
                                                                text += ": " + roleDef.Name;
                                                                value += list.ID;
                                                                break;
                                                            }
                                                        }
                                                    }

                                                }
                                            }
                                            if (!groupsText.Equals(""))
                                            {
                                                MILRemovingLblPanel.Controls.Add(new LiteralControl("<br />"));
                                                MILRemovingLblPanel.Controls.Add(label);

                                            }
                                            if (value != "")
                                            {
                                                ListItem newItem = new ListItem();
                                                newItem.Value = value;
                                                newItem.Text = text;
                                                MILRemoveListBox.Items.Add(newItem);
                                            }
                                        }

                                    }

                                }
                                else if (MILtypeDD.Text == "Sites")
                                {
                                    foreach (ListItem item in MILobjectList.Items)
                                    {
                                        if (item.Selected)
                                        {
                                            Guid id = new Guid(item.Value);
                                            SPWeb web = mysite.AllWebs[id];
                                            String text = web.Title;
                                            String value = "";
                                            Label label = new Label();
                                            String groupsText = "";
                                            SPPermissionInfo info = web.GetUserEffectivePermissionInfo(user.userAcct.LoginName);
                                            foreach (SPRoleAssignment role in info.RoleAssignments)
                                            {
                                                foreach (SPRoleDefinition roleDef in role.RoleDefinitionBindings)
                                                {
                                                    if (!roleDef.Name.Equals("Limited Access") && !roleDef.Name.Equals("Approve") && !roleDef.Name.Equals("Manage Hierarchy") && !roleDef.Name.Equals("Restricted Read"))
                                                    {
                                                        if (role.Member is SPGroup)
                                                        {
                                                            SPGroup group = (SPGroup)role.Member;
                                                            if (groupsText.Equals(""))
                                                            {
                                                                groupsText = "*WARNING* user is part of: ";
                                                            }
                                                            groupsText += "\"" + group.Name + "\", ";
                                                        }
                                                        else
                                                        {
                                                            SPUser user2 = (SPUser)role.Member;
                                                            if (user2.IsDomainGroup)
                                                            {
                                                                if (groupsText.Equals(""))
                                                                {
                                                                    groupsText = "*WARNING* user is part of: ";
                                                                }
                                                                groupsText += "\"" + user2.Name + "\"(Active Directory Group), ";
                                                            }
                                                            else
                                                            {
                                                                text += ": " + roleDef.Name;
                                                                value += web.ID;
                                                                break;
                                                            }
                                                        }
                                                    }

                                                }

                                            }
                                            if (!groupsText.Equals(""))
                                            {
                                                MILRemovingLblPanel.Controls.Clear();
                                                groupsText = groupsText.Remove(groupsText.Length - 2);
                                                groupsText += " group(s) and will still have access to " + web.Title;
                                                label.Text += groupsText;
                                                MILRemovingLblPanel.Controls.Add(new LiteralControl("<br />"));
                                                MILRemovingLblPanel.Controls.Add(label);

                                            }
                                            if (value != "")
                                            {

                                                ListItem newItem = new ListItem();
                                                newItem.Value = value;
                                                newItem.Text = text;
                                                MILRemoveListBox.Items.Add(newItem);
                                            }

                                        }
                                    }
                                }
                                else
                                {
                                    foreach (ListItem item in MILobjectList.Items)
                                    {
                                        if (item.Selected)
                                        {
                                            //Guid id = new Guid(item.Value);
                                            SPGroup group = myweb.Groups.GetByID(Int32.Parse(item.Value));
                                            String text = group.Name;
                                            String value = "";
                                            Label label = new Label();
                                            foreach (SPGroup groups in user.userAcct.Groups)
                                            {
                                                if (groups.Name.Equals(group.Name))
                                                {
                                                    value = group.ID.ToString();
                                                    break;
                                                }
                                            }
                                            if (value != "")
                                            {

                                                ListItem newItem = new ListItem();
                                                newItem.Value = value;
                                                newItem.Text = text;
                                                MILRemoveListBox.Items.Add(newItem);

                                            }

                                        }
                                    }
                                    //MILRemovingLblPanel.Controls.Clear();
                                }
                            }
                            else
                            {
                                MILRemoveErrorLabel.Text = "You must have 1 user selected";
                                if (MILactionDD.Text != ("Removing"))
                                {
                                    removeTransCol.transactionList.Clear();
                                    ViewState["removeTransCol"] = removeTransCol;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            //log Error
                        }
                    }
                });
            }
            else
            {
                MILRemoveErrorLabel.Text = "You must have 1 user selected, and one site/list/group selected as well";                
            }
        }


        //TODO needs review
        private void buildPermsTable()
        {
            SPSite mysite = SPContext.Current.Site;

            using (SPWeb myweb = mysite.OpenWeb())
            {

                MILAddPermListBox.Items.Clear();
                //userPermissions newUP = new userPermissions(myweb, null);
                int ctr = 0;
                foreach (SPRoleDefinition roleDef in myweb.RoleDefinitions)
                {
                    //"Limited Access" "Approve" "Manage Hierarchy" "Restricted Read"
                    if (!roleDef.Name.Equals("Limited Access") && !roleDef.Name.Equals("Approve") && !roleDef.Name.Equals("Manage Hierarchy") && !roleDef.Name.Equals("Restricted Read"))
                    {
                        ctr++;
                        ListItem item = new ListItem();
                        item.Value = roleDef.Name;
                        item.Text = roleDef.Name;
                        MILAddPermListBox.Items.Add(item);
                    }
                }
                MILAddUpdPnl.Update();
            }
        }


        private void buildTable()
        {
            //SPWebApplication webApp = SPContext.Current.Site.WebApplication;

            SPSite mysite = SPContext.Current.Site;

            using (SPWeb myweb = mysite.OpenWeb())
            {
                if (myweb.DoesUserHavePermissions(perms))
                { 
                    MILobjectList.Items.Clear();
                    if (MILtypeDD.SelectedValue == "Lists")
                    {
                        SPListCollection listCol = myweb.Lists;

                        foreach (SPList list in listCol)
                        {
                            if (list.DoesUserHavePermissions(perms))
                            {
                                ListItem item = new ListItem();
                                item.Value = list.ID.ToString();
                                item.Text = list.Title;
                                MILobjectList.Items.Add(item);
                            }
                        }
                    }
                    else if (MILtypeDD.SelectedValue == "Sites")
                    {
                        SPWebCollection webCol = mysite.AllWebs;
                        foreach (SPWeb web in webCol)
                        {
                            if (web.DoesUserHavePermissions(perms))
                            {
                                ListItem item = new ListItem();
                                item.Value = web.ID.ToString();
                                item.Text = web.Title;
                                MILobjectList.Items.Add(item);
                            }
                        }
                    }
                    else
                    {
                        SPGroupCollection groupCol = myweb.Groups;
                        foreach (SPGroup group in groupCol)
                        {
                            if (myweb.DoesUserHavePermissions(perms))
                            {
                                ListItem item = new ListItem();
                                item.Value = group.ID.ToString();
                                item.Text = group.Name;
                                MILobjectList.Items.Add(item);
                            }
                        }
                    }
                    MILObjectUpdPnl.Update();
                    MILSpecificsUpdPnl.Update();
                }
            }
        }
        private void buildRemovingTransTable(object sender, EventArgs e)
        {

            if (removeTransCol != null && removeTransCol.transactionList.Count > 0)
            {
                MILRemovingTable.Rows.Clear();
                TableCell userTitleCell = new TableCell();
                TableCell objectTitleCell = new TableCell();
                TableRow headerRow = new TableRow();
                userTitleCell.Text = "User";
                objectTitleCell.Text = "Removing from";
                headerRow.Cells.Add(userTitleCell);
                headerRow.Cells.Add(objectTitleCell);
                MILRemovingTable.Rows.Add(headerRow);
                foreach (Transaction trans in removeTransCol.transactionList)
                {
                    TableCell userNameCell = new TableCell();
                    TableCell objectCell = new TableCell();
                    TableRow row = new TableRow();
                    SPUser user = SPContext.Current.Web.SiteUsers.GetByID(trans.userID);
                    userNameCell.Text = user.Name;
                    objectCell.Text = trans.objectName + "(" + trans.objectType + ")";
                    row.Cells.Add(userNameCell);
                    row.Cells.Add(objectCell);
                    MILRemovingTable.Rows.Add(row);

                }
            }
        }
        private void buildAddingTransTable(object sender, EventArgs e)
        {

            if (addTransCol != null && addTransCol.transactionList.Count > 0)
            {
                MILAddingTransaction.Rows.Clear();
                TableCell userTitleCell = new TableCell();
                TableCell objectTitleCell = new TableCell();
                TableRow headerRow = new TableRow();
                userTitleCell.Text = "User";
                objectTitleCell.Text = "Adding from";
                headerRow.Cells.Add(userTitleCell);
                headerRow.Cells.Add(objectTitleCell);
                MILAddingTransaction.Rows.Add(headerRow);
                foreach (Transaction trans in addTransCol.transactionList)
                {
                    TableCell userNameCell = new TableCell();
                    TableCell objectCell = new TableCell();
                    TableRow row = new TableRow();
                    SPUser user = SPContext.Current.Web.SiteUsers.GetByID(trans.userID);
                    userNameCell.Text = user.Name;
                    objectCell.Text = trans.objectName + "(" + trans.objectType + ")";
                    row.Cells.Add(userNameCell);
                    row.Cells.Add(objectCell);
                    MILAddingTransaction.Rows.Add(row);

                }
            }
            MILAddUpdPnl.Update();
        }
        #endregion


        #region Add Functions
        protected void AddPermissions(object sender, EventArgs e)
        {
            if(addTransCol==null)
            {
                addTransCol = new TransactionCollection();
            }
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                SPSite mysite = SPContext.Current.Site;


                using (SPWeb myweb = mysite.OpenWeb())
                {
                    if (MILobjectList.Items.Count > 0 && MILAddPermListBox.Items.Count > 0 && MILPPicker.ResolvedEntities.Count > 0)
                    {
                        if (MILtypeDD.Text == "Lists")
                        {
                            foreach (ListItem item in MILobjectList.Items)
                            {
                                if (item.Selected)
                                {
                                    Guid id = new Guid(item.Value);
                                    SPList list = myweb.Lists[id];
                                    foreach (PickerEntity user in MILPPicker.ResolvedEntities)
                                    {
                                        SPUser spUser = myweb.EnsureUser(user.Key);
                                        foreach (ListItem perms in MILAddPermListBox.Items)
                                        {
                                            if (perms.Selected)
                                            {
                                                addTransCol.Add(spUser, list, perms.Value);

                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (MILtypeDD.Text == "Sites")
                        {
                            foreach (ListItem item in MILobjectList.Items)
                            {
                                if (item.Selected)
                                {
                                    Guid id = new Guid(item.Value);
                                    SPWeb web = mysite.AllWebs[id];

                                    foreach (PickerEntity user in MILPPicker.ResolvedEntities)
                                    {
                                        SPUser spUser = myweb.EnsureUser(user.Key);

                                        foreach (ListItem perms in MILAddPermListBox.Items)
                                        {
                                            if (perms.Selected)
                                            {
                                                addTransCol.Add(spUser, web, perms.Value);

                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (ListItem item in MILobjectList.Items)
                            {
                                if (item.Selected)
                                {

                                    int id = Int32.Parse(item.Value);
                                    foreach (PickerEntity user in MILPPicker.ResolvedEntities)
                                    {
                                        SPUser spUser = myweb.EnsureUser(user.Key);

                                        SPGroup group = myweb.Groups.GetByID(id);
                                        addTransCol.Add(spUser, group, null);

                                    }
                                }
                            }
                        }
                        //CompleteAddTransaction(sender, e);
                        ViewState["addTransCol"] = addTransCol;
                        buildAddingTransTable(sender, e);
                    }
                }
            });
        }
        protected void CompleteAddTransaction(object sender, EventArgs e)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                if (addTransCol != null)
                {
                    foreach (Transaction trans in addTransCol.transactionList)
                    {

                        if (trans.objectType == "Web")
                        {
                            //Guid guid = new Guid(trans.spObject);
                            using (SPSite site = new SPSite(trans.spObject))
                            {
                                using (SPWeb web = site.OpenWeb())
                                {
                                    SPUser spUser = web.SiteUsers.GetByID(trans.userID);
                                    if (!web.HasUniqueRoleAssignments)
                                    {
                                        web.AllowUnsafeUpdates = true;
                                        web.Update();
                                        web.BreakRoleInheritance(true);
                                    }
                                    SPRoleDefinition roleDef = web.RoleDefinitions[trans.roleDefName];
                                    SPRoleAssignment roleAssignment = new SPRoleAssignment(spUser.LoginName, spUser.Email, spUser.Name, "Added through permissions manager tool");
                                    roleAssignment.RoleDefinitionBindings.Add(roleDef);
                                    web.AllowUnsafeUpdates = true;
                                    web.RoleAssignments.Add(roleAssignment);
                                    web.Update();
                                    web.AllowUnsafeUpdates = false;
                                }
                            }
                        }
                        else if (trans.objectType == "List")
                        {
                            using (SPSite site = new SPSite(trans.parentURL))
                            {
                                using (SPWeb web = site.OpenWeb())
                                {
                                    SPUser spUser = web.SiteUsers.GetByID(trans.userID);
                                    SPList list = web.Lists[trans.spObject];
                                    list.ParentWeb.AllowUnsafeUpdates = true;
                                    if (!list.HasUniqueRoleAssignments)
                                    {
                                        list.Update();
                                        list.BreakRoleInheritance(true);//security validation was invalid
                                    }
                                    SPRoleDefinition roleDef = web.RoleDefinitions[trans.roleDefName];
                                    SPRoleAssignment roleAssignment = new SPRoleAssignment(spUser.LoginName, spUser.Email, spUser.Name, "Added through permissions manager tool");
                                    roleAssignment.RoleDefinitionBindings.Add(roleDef);
                                    list.ParentWeb.AllowUnsafeUpdates = true;
                                    list.RoleAssignments.Add(roleAssignment);
                                    list.Update();
                                    list.ParentWeb.AllowUnsafeUpdates = false;
                                }
                            }
                        }
                        else
                        {
                            using (SPSite site = new SPSite(trans.parentURL))
                            {
                                using (SPWeb web = site.OpenWeb())
                                {
                                    SPUser spUser = web.SiteUsers.GetByID(trans.userID);
                                    SPGroup group = web.Groups[trans.spObject];
                                    group.ParentWeb.AllowUnsafeUpdates = true;
                                    group.AddUser(spUser);
                                    group.Update();
                                    group.ParentWeb.AllowUnsafeUpdates = false;
                                }
                            }
                        }
                    }
                    addTransCol.transactionList.Clear();
                    MILAddPermListBox.Items.Clear();
                    ViewState["addTransCol"] = addTransCol;
                    MILAddErrorLabel.Text = "Success, completed Transaction";
                }
            });
        }
        #endregion


        #region Remove Functions
        protected void removePermissions(object sender, EventArgs e)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate ()
            {
                SPSite mysite = SPContext.Current.Site;

                using (SPWeb myweb = mysite.OpenWeb())
                {
                    UserData user = getUserFromPP(MILPPicker)[0];

                    if (user != null)
                    {
                        if (removeTransCol == null)
                        {
                            removeTransCol = new TransactionCollection();
                        }
                        if (MILtypeDD.Text == "Lists")
                        {
                            foreach (ListItem item in MILRemoveListBox.Items)
                            {
                                if (item.Selected)
                                {
                                    Guid id = new Guid(item.Value);
                                    SPList list = myweb.Lists[id];
                                    removeTransCol.Add(user.userAcct, list, null);
                                }
                            }
                        }
                        else if (MILtypeDD.Text == "Sites")
                        {
                            foreach (ListItem item in MILRemoveListBox.Items)
                            {
                                if (item.Selected)
                                {
                                    //now change to work with items selected from perms 
                                    Guid id = new Guid(item.Value);
                                    SPWeb web = mysite.AllWebs[id];

                                    removeTransCol.Add(user.userAcct, web, null);
                                }
                            }
                        }
                        else
                        {
                            foreach (ListItem item in MILRemoveListBox.Items)
                            {
                                if (item.Selected)
                                {
                                    //Guid id = new Guid(item.Value);
                                    int id = Int32.Parse(item.Value);
                                    SPGroup group = myweb.Groups.GetByID(id);
                                    removeTransCol.Add(user.userAcct, group, null);
                                    //group.ParentWeb.AllowUnsafeUpdates = true;
                                    //group.RemoveUser(user.userAcct);
                                    //group.Update();
                                    //group.ParentWeb.AllowUnsafeUpdates = false;


                                }
                            }
                        }
                        ViewState["removeTransCol"] = removeTransCol;
                        buildRemovingTransTable(sender, e);
                    }
                }
            });

        }
        protected void CompleteRemoveTransaction(object sender, EventArgs e)
        {
            if (removeTransCol != null)
            {
                foreach (Transaction trans in removeTransCol.transactionList)
                {
                    if (trans.objectType == "Web")
                    {
                        using (SPSite site = new SPSite(trans.spObject))
                        {
                            using (SPWeb web = site.OpenWeb())
                            {
                                SPUser spUser = web.SiteUsers.GetByID(trans.userID);
                                web.AllowUnsafeUpdates = true;
                                if (!web.HasUniqueRoleAssignments)
                                {
                                    web.Update();
                                    web.BreakRoleInheritance(true);
                                }
                                web.AllowUnsafeUpdates = true;
                                web.RoleAssignments.Remove(spUser);
                                web.Update();
                                web.AllowUnsafeUpdates = false;
                            }
                        }
                    }
                    else if (trans.objectType == "List")
                    {
                        using (SPSite site = new SPSite(trans.parentURL))
                        {
                            using (SPWeb web = site.OpenWeb())
                            {
                                SPUser spUser = web.SiteUsers.GetByID(trans.userID);
                                //Guid guid = new Guid(trans.spObject);
                                SPList list = web.Lists[trans.spObject];
                                list.ParentWeb.AllowUnsafeUpdates = true;
                                if (!list.HasUniqueRoleAssignments)
                                {
                                    list.Update();
                                    list.BreakRoleInheritance(true);//security validation was invalid
                                }
                                list.ParentWeb.AllowUnsafeUpdates = true;
                                list.RoleAssignments.Remove(spUser);
                                list.Update();
                                list.ParentWeb.AllowUnsafeUpdates = false;
                            }
                        }
                    }
                    else
                    {
                        using (SPSite site = new SPSite(trans.parentURL))
                        {
                            using (SPWeb web = site.OpenWeb())
                            {
                                SPUser spUser = web.SiteUsers.GetByID(trans.userID);
                                SPGroup group = web.Groups[trans.spObject];
                                group.ParentWeb.AllowUnsafeUpdates = true;
                                group.RemoveUser(spUser);
                                group.Update();
                                group.ParentWeb.AllowUnsafeUpdates = false;
                            }
                        }
                    }
                }
                removeTransCol.transactionList.Clear();
                MILRemovingLblPanel.Controls.Clear();
                MILAddPermListBox.Items.Clear();
                ViewState["removeTransCol"] = removeTransCol;
                loadUserPermsTable(sender, e);
                MILRemoveErrorLabel.Text += "Success, Complated transaction";
            }
        }
        #endregion


        #region Comparing Functions
        /// <summary>
        /// Onclick handler for the compare user button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void compareUsers(object sender, EventArgs e)
        {
            SPSite mysite = SPContext.Current.Site;
            using (SPWeb myweb = mysite.OpenWeb())
            {
                UserData[] user = getUserFromPP(MILPPicker);
                user = checkingUsers(myweb, user);

                buildComparisonTable(user[0], user[1], MILCompareTable);
            }
        }
        #endregion


        #region LINQ stuff
        private void buildComparingTableWithLinq(UserData user1, UserData user2, Table table)
        {

            List<String> objectNames = new List<String>();
            TableRow row1 = new TableRow();
            TableCell siteTitle = new TableCell();
            siteTitle.ColumnSpan = 3;
            siteTitle.Text = "Sites";
            siteTitle.Style.Add("font-weight", "bold");
            row1.Cells.Add(siteTitle);
            table.Rows.Add(row1);
            IEnumerable<userPermissions> permQuerySiteU1 =
                from userPerms in user1.userPerms
                where userPerms.objectType == "Web"
                select userPerms;
            objectNames = BuildSiteTableLinq(permQuerySiteU1, user2, objectNames, "1", table);
            IEnumerable<userPermissions> permQuerySiteU2 =
                from userPerms in user2.userPerms
                where userPerms.objectType == "Web" && !comparingUPs.Contains(userPerms)
                select userPerms;
            objectNames = BuildSiteTableLinq(permQuerySiteU2, user1, objectNames, "2", table);
            TableRow row2 = new TableRow();
            TableCell listTitle = new TableCell();
            listTitle.ColumnSpan = 3;
            listTitle.Text = "Lists";
            listTitle.Style.Add("font-weight", "bold");
            row2.Cells.Add(listTitle);
            table.Rows.Add(row2);

            IEnumerable<userPermissions> permQueryListU1 =
                from userPerms in user1.userPerms
                where userPerms.objectType == "List"
                select userPerms;
            objectNames = BuildListTableLinq(permQueryListU1, user2, objectNames, "1", table);
            IEnumerable<userPermissions> permQueryListU2 =
                from userPerms in user2.userPerms
                where userPerms.objectType == "List" && !comparingUPs.Contains(userPerms)
                select userPerms;
            objectNames = BuildListTableLinq(permQueryListU2, user1, objectNames, "2", table);

            TableRow row3 = new TableRow();
            TableCell itemTitle = new TableCell();
            itemTitle.ColumnSpan = 3;
            itemTitle.Text = "Items";
            itemTitle.Style.Add("font-weight", "bold");
            row3.Cells.Add(itemTitle);
            table.Rows.Add(row3);

            IEnumerable<userPermissions> permQueryItemU1 =
                from userPerms in user1.userPerms
                where userPerms.objectType == "Item"
                select userPerms;
            objectNames = BuildItemTableLinq(permQueryItemU1, user2, objectNames, "1", table);
            IEnumerable<userPermissions> permQueryItemU2 =
                from userPerms in user2.userPerms
                where userPerms.objectType == "Item" && !comparingUPs.Contains(userPerms)
                select userPerms;
            objectNames = BuildItemTableLinq(permQueryItemU2, user1, objectNames, "2", table);

        }
        private List<String> BuildSiteTableLinq(IEnumerable<userPermissions> Query1, UserData user2, List<String> objectNames, String user, Table table)
        {


            foreach (userPermissions up in Query1)
            {
                if (!comparingUPs.Contains(up))
                {
                    TableCell itemTitle = new TableCell();
                    TableCell permsU1 = new TableCell();
                    TableCell permsU2 = new TableCell();
                    TableRow row = new TableRow();
                    itemTitle.Text = up.web.Url;
                    objectNames.Add(up.web.Url);
                    String roleDefName = "";
                    foreach (SPRoleDefinition rd in up.roleDef)
                    {
                        if ((!comparingUPs.Contains(up)))
                        {
                            permsU1.Text += rd.Name;
                            roleDefName = rd.Name;
                            if (up.isGroup)
                            {
                                SPGroup group = (SPGroup)up.roleAssignment.Member;
                                permsU1.Text += " given through \"" + group.Name + "\", ";
                            }
                            else if (up.isADGroup)
                            {
                                SPUser adUser = (SPUser)up.roleAssignment.Member;
                                permsU1.Text += " given through AD Group \"" + adUser.Name + "\", ";
                            }
                            else
                            {
                                permsU1.Text += " given Directly, ";
                            }
                            comparingUPs.Add(up);
                            //break;
                        }
                    }
                    IEnumerable<userPermissions> permQerySiteU2 =
                        from userPerms in user2.userPerms
                        where userPerms.objectType == "Web" && userPerms.web.Url == up.web.Url
                        select userPerms;
                    if (permQerySiteU2.Count() > 0)
                    {
                        int i = 0;
                        foreach (userPermissions QueryResults in permQerySiteU2)
                        {
                            i++;
                            //userPermissions QueryResults = permQerySiteU2.First();
                            foreach (SPRoleDefinition rd in QueryResults.roleDef)
                            {

                                if ((!comparingUPs.Contains(QueryResults) && rd.Name.Equals(roleDefName)) || permQerySiteU2.Count() == i)
                                {
                                    permsU2.Text += rd.Name;
                                    if (QueryResults.isGroup)
                                    {
                                        SPGroup group = (SPGroup)QueryResults.roleAssignment.Member;
                                        permsU2.Text += " given through \"" + group.Name + "\", ";
                                    }
                                    else if (QueryResults.isADGroup)
                                    {
                                        SPUser adUser = (SPUser)QueryResults.roleAssignment.Member;
                                        permsU2.Text += " given through AD Group \"" + adUser.Name + "\", ";
                                    }
                                    else
                                    {
                                        permsU2.Text += " given Directly, ";
                                    }
                                    //break;
                                    comparingUPs.Add(QueryResults);
                                }
                            }
                            if (permsU2.Text != "")
                            {
                                break;
                            }
                        }

                    }
                    if (permsU1.Text.Equals(permsU2.Text))
                    {
                        //still to implement: highlighting the objects that are different and copy same function to list and item 
                        //permsU1.
                    }
                    row.Cells.Add(itemTitle);
                    if (permsU1.Text.Length > 2)
                    {
                        permsU1.Text = permsU1.Text.Remove(permsU1.Text.Length - 2);
                    }
                    if (permsU2.Text.Length > 2)
                    {
                        permsU2.Text = permsU2.Text.Remove(permsU2.Text.Length - 2);
                    }
                    if (user == "1")
                    {
                        row.Cells.Add(permsU1);
                        row.Cells.Add(permsU2);
                    }
                    else
                    {
                        row.Cells.Add(permsU2);
                        row.Cells.Add(permsU1);
                    }
                    table.Rows.Add(row);
                    //}
                }
            }
            return objectNames;
        }
        private List<String> BuildListTableLinq(IEnumerable<userPermissions> Query1, UserData user2, List<String> objectNames, String user, Table table)
        {
            foreach (userPermissions up in Query1)
            {
                if (!up.list.Hidden)
                {
                    if (!comparingUPs.Contains(up))
                    {
                        TableCell itemTitle = new TableCell();
                        TableCell permsU1 = new TableCell();
                        TableCell permsU2 = new TableCell();
                        TableRow row = new TableRow();
                        itemTitle.Text = up.list.Title;
                        objectNames.Add(up.list.Title);
                        String roleDefName = "";
                        foreach (SPRoleDefinition rd in up.roleDef)
                        {
                            if ((!comparingUPs.Contains(up)))
                            {
                                permsU1.Text += rd.Name;
                                roleDefName = rd.Name;
                                if (up.isGroup)
                                {
                                    SPGroup group = (SPGroup)up.roleAssignment.Member;
                                    permsU1.Text += " given through \"" + group.Name + "\", ";
                                }
                                else if (up.isADGroup)
                                {
                                    SPUser adUser = (SPUser)up.roleAssignment.Member;
                                    permsU1.Text += " given through AD Group \"" + adUser.Name + "\", ";
                                }
                                else
                                {
                                    permsU1.Text += " given Directly, ";
                                }
                                comparingUPs.Add(up);
                                //break;
                            }
                        }
                        IEnumerable<userPermissions> permQerySiteU2 =
                            from userPerms in user2.userPerms
                            where userPerms.objectType == "List" && userPerms.list.Title == up.list.Title
                            select userPerms;
                        if (permQerySiteU2.Count() > 0)
                        {
                            int i = 0;
                            foreach (userPermissions QueryResults in permQerySiteU2)
                            {
                                i++;
                                //userPermissions QueryResults = permQerySiteU2.First();
                                foreach (SPRoleDefinition rd in QueryResults.roleDef)
                                {
                                    if ((!comparingUPs.Contains(QueryResults) && rd.Name.Equals(roleDefName)) || permQerySiteU2.Count() == i)
                                    {
                                        permsU2.Text += rd.Name;
                                        if (QueryResults.isGroup)
                                        {
                                            SPGroup group = (SPGroup)QueryResults.roleAssignment.Member;
                                            permsU2.Text += " given through \"" + group.Name + "\", ";
                                        }
                                        else if (QueryResults.isADGroup)
                                        {
                                            SPUser adUser = (SPUser)QueryResults.roleAssignment.Member;
                                            permsU2.Text += " given through AD Group \"" + adUser.Name + "\", ";
                                        }
                                        else
                                        {
                                            permsU2.Text += " given Directly, ";
                                        }
                                        comparingUPs.Add(QueryResults);
                                        //break;
                                    }
                                }
                                if (permsU2.Text != "")
                                {
                                    break;
                                }
                            }
                        }
                        row.Cells.Add(itemTitle);
                        if (permsU1.Text.Length > 2)
                        {
                            permsU1.Text = permsU1.Text.Remove(permsU1.Text.Length - 2);
                        }
                        if (permsU2.Text.Length > 2)
                        {
                            permsU2.Text = permsU2.Text.Remove(permsU2.Text.Length - 2);
                        }
                        if (user == "1")
                        {
                            row.Cells.Add(permsU1);
                            row.Cells.Add(permsU2);
                        }
                        else
                        {
                            row.Cells.Add(permsU2);
                            row.Cells.Add(permsU1);
                        }
                        table.Rows.Add(row);
                    }
                }

            }

            return objectNames;//i dont think there is anything to add here but integrate same thing for item list
        }
        private List<String> BuildItemTableLinq(IEnumerable<userPermissions> Query1, UserData user2, List<String> objectNames, String user, Table table)
        {
            foreach (userPermissions up in Query1)
            {

                if (!comparingUPs.Contains(up))
                {
                    TableCell itemTitle = new TableCell();
                    TableCell permsU1 = new TableCell();
                    TableCell permsU2 = new TableCell();
                    String itemText = up.item.ParentList.Title + ": " + up.item.DisplayName;
                    TableRow row = new TableRow();
                    itemTitle.Text = itemText;
                    objectNames.Add(itemText);
                    String roleDefName = "";
                    foreach (SPRoleDefinition rd in up.roleDef)
                    {
                        if ((!comparingUPs.Contains(up)))
                        {
                            permsU1.Text += rd.Name;
                            roleDefName = rd.Name;
                            if (up.isGroup)
                            {
                                SPGroup group = (SPGroup)up.roleAssignment.Member;
                                permsU1.Text += " given through \"" + group.Name + "\", ";
                            }
                            else if (up.isADGroup)
                            {
                                SPUser adUser = (SPUser)up.roleAssignment.Member;
                                permsU1.Text += " given through AD Group \"" + adUser.Name + "\", ";
                            }
                            else
                            {
                                permsU1.Text += " given Directly, ";
                            }
                            //break;
                            comparingUPs.Add(up);
                        }
                    }
                    IEnumerable<userPermissions> permQerySiteU2 =
                        from userPerms in user2.userPerms
                        where userPerms.objectType == "Item" && userPerms.item.DisplayName == up.item.DisplayName
                        select userPerms;
                    //userPermissions user2Query1 = user2.userPerms.First(userPerms => userPerms.item.ParentList.Title + ": "+ userPerms.item.DisplayName == itemText);
                    if (permQerySiteU2.Count() > 0)
                    {
                        int i =0;
                        foreach (userPermissions QueryResults in permQerySiteU2)
                        {
                            i++;
                            //userPermissions QueryResults = permQerySiteU2.First();
                            foreach (SPRoleDefinition rd in QueryResults.roleDef)
                            {
                                if ((!comparingUPs.Contains(QueryResults) && rd.Name.Equals(roleDefName)) || permQerySiteU2.Count() == i)
                                {
                                    permsU2.Text += rd.Name;
                                    if (QueryResults.isGroup)
                                    {
                                        SPGroup group = (SPGroup)QueryResults.roleAssignment.Member;
                                        permsU2.Text += " given through \"" + group.Name + "\", ";
                                    }
                                    else if (QueryResults.isADGroup)
                                    {
                                        SPUser adUser = (SPUser)QueryResults.roleAssignment.Member;
                                        permsU2.Text += " given through AD Group \"" + adUser.Name + "\", ";
                                    }
                                    else
                                    {
                                        permsU2.Text += " given Directly, ";
                                    }
                                    //break;
                                    comparingUPs.Add(QueryResults);
                                }
                            }
                            if(permsU2.Text!="")
                            {
                                break;
                            }
                        }
                    }
                    row.Cells.Add(itemTitle);
                    if (permsU1.Text.Length > 2)
                    {
                        permsU1.Text = permsU1.Text.Remove(permsU1.Text.Length - 2);
                    }
                    if (permsU2.Text.Length > 2)
                    {
                        permsU2.Text = permsU2.Text.Remove(permsU2.Text.Length - 2);
                    }
                    if (user == "1")
                    {
                        row.Cells.Add(permsU1);
                        row.Cells.Add(permsU2);
                    }
                    else
                    {
                        row.Cells.Add(permsU2);
                        row.Cells.Add(permsU1);
                    }
                    table.Rows.Add(row);
                }
            }

            return objectNames;
        }
        #endregion


        #region Replacing Functions

        /// <summary>
        /// compare two users together to see differences
        /// useful to see before replacing a user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void showAllData(Object sender, EventArgs e)
        {
            UserData[] userList = new UserData[2];

            userList[0] = getUserFromPP(MILReplaceOldUserPP)[0];
            userList[1] = getUserFromPP(MILReplaceNewUserPP)[0];

            if (userList[0] == null || userList[1] == null)
            {
                MILReplaceMsg.Text = "You must enter exactly 2 people";

                MILReplaceUpdPnl.Update();
            }
            else
            {
                MILReplaceMsg.Text = "Replace";

                UserData[] newUser = checkingUsers(Web, userList);

                buildComparisonTable(newUser[0], newUser[1], MILReplaceTable);

                MILReplaceUpdPnl.Update();
            }
        }


        /// <summary>
        /// On click event handler for the replacing users button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void replaceUsers(Object sender, EventArgs e)
        {
            String cur = SPContext.Current.Web.Url;

            SPSite mysite = SPContext.Current.Site;
            {
                using (SPWeb myweb = mysite.OpenWeb())
                {
                    UserData user1 = getUserFromPP(MILReplaceOldUserPP)[0];
                    UserData user2 = getUserFromPP(MILReplaceNewUserPP)[0];
                    UserData[] users = { user1, user2 };
                    users = checkingUsers(myweb, users);
                    if (user1.userPerms != null && user2.userPerms != null)
                    {
                        foreach (userPermissions userPerms in user1.userPerms)
                        {
                            foreach (SPRoleDefinition roleDef in userPerms.roleDef)
                            {
                                replace(user1, user2, userPerms, roleDef);
                            }

                        }
                    }
                }
            }
        }


        //function to make my life easier
        /// <summary>
        ///pass in information about a users permission on that 'thing', with the new user
        ///assign same role definition on the new user on the same 'thing'
        /// </summary>
        /// <param name="oldUser">The old user going away</param>
        /// <param name="newUser">The new user taking the old users place</param>
        /// <param name="data">information about the old users permissions</param>
        /// <param name="role">the role definition for that site</param>

        private void replace(UserData oldUser, UserData newUser, userPermissions data, SPRoleDefinition role)
        {
            SPRoleAssignment assign = new SPRoleAssignment(newUser.userAcct);

            //TODO add logging to this

            try
            {
                assign.RoleDefinitionBindings.Add(role);

                switch (data.objectType)
                {
                    case "Web":
                        SPWeb web = data.web;
                        web.RoleAssignments.Add(assign);
                        web.RoleAssignments.Remove(oldUser.userAcct);
                        break;

                    case "List":
                        SPList list = data.list;
                        if (list.HasUniqueRoleAssignments)
                        {
                            list.RoleAssignments.Add(assign);
                            list.RoleAssignments.Remove(oldUser.userAcct);
                        }
                        else
                        {
                            if (list.ParentWeb.HasUniqueRoleAssignments)
                            {
                                list.ParentWeb.RoleAssignments.Add(assign);
                                list.RoleAssignments.Remove(oldUser.userAcct);
                            }
                        }
                        break;

                    case "Item":
                        SPListItem item = data.item;
                        if (item.HasUniqueRoleAssignments)
                        {
                            item.RoleAssignments.Add(assign);
                            item.RoleAssignments.Remove(oldUser.userAcct);
                        }
                        else
                        {
                            if (item.ParentList.HasUniqueRoleAssignments)
                            {
                                item.ParentList.RoleAssignments.Add(assign);
                                item.ParentList.RoleAssignments.Remove(oldUser.userAcct);
                            }
                            else
                            {
                                item.ParentList.ParentWeb.RoleAssignments.Add(assign);
                                item.ParentList.RoleAssignments.Remove(oldUser.userAcct);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (SPException)
            {
                //tryed to assign the limited access role, cant do that
                //so instead i do nothing
                //for i am nothing
            }
        }
        #endregion


        #region Utility Functions
        /// <summary>
        /// function to remove all null users from an array
        /// </summary>
        /// <param name="users">Array with null garbage in it</param>
        /// <returns>Array with no garbage in it</returns>
        private IEnumerable<UserData> RemoveAllNull(IEnumerable<UserData> users)
        {
            return users.Where(u => u != null).ToArray();
        }
        /// <summary>
        /// //TODO add description
        /// </summary>
        /// <param name="user1"></param>
        /// <param name="user2"></param>
        /// <param name="table"></param>
        private void buildComparisonTable(UserData user1, UserData user2, Table table)
        {
            //cant put in using block, breaks lots of stuff
            MILCompareErrorLabel.Text = "";
            if (user1 != null && user2 != null)
            {
                SPSite mysite = SPContext.Current.Site;

                using (SPWeb myweb = mysite.OpenWeb())
                {
                    TableCell objectTitle = new TableCell();
                    TableCell user1Title = new TableCell();
                    TableCell user2Title = new TableCell();

                    objectTitle.Text = "Objects";
                    try
                    {
                        user1Title.Text = user1.userAcct.Name;
                    }
                    catch (Exception)
                    {
                        //try to get their name, otherwise use placeholder
                        user1Title.Text = "User 1";
                    }

                    try
                    {
                        user2Title.Text = user2.userAcct.Name;
                    }
                    catch (Exception)
                    {
                        //try to get their name, otherwise use placeholder
                        user1Title.Text = "User 1";
                    }

                    TableRow header = new TableRow();
                    header.Cells.Add(objectTitle);
                    header.Cells.Add(user1Title);
                    header.Cells.Add(user2Title);

                    table.Rows.Add(header);

                    buildComparingTableWithLinq(user1, user2, table);
                }
            }
            else
            {
                MILCompareErrorLabel.Text = "Error- you must have at least two users selected";
            }
        }


        /// <summary>
        /// Gets the user object associated with the people picked in the people picker
        /// </summary>
        /// <param name="peoplePicker">sharepoint people picker</param>
        /// <returns>userData of the people picked in the people picker</returns>
        private UserData[] getUserFromPP(PeopleEditor peoplePicker)
        {
            UserData[] userST = new UserData[10];
            SPSite mysite = SPContext.Current.Site;

            using (SPWeb myweb = mysite.OpenWeb())
            {
                int peoplePickerCount = peoplePicker.ResolvedEntities.Count;
                if (peoplePicker.ResolvedEntities.Count > 0)
                {
                    for(int i = 0; i < peoplePicker.ResolvedEntities.Count; i++)
                    { 
                        PickerEntity user = (PickerEntity)peoplePicker.ResolvedEntities[i];

                        if (user.EntityData["PrincipalType"].ToString() == "User")
                        {
                            //get the users specified in the peoplepicker

                            SPUser selectedUser = myweb.EnsureUser(user.Key);
                            userST[i] = new UserData(selectedUser);
                        }
                        else
                        {
                            //principaltype is group or something else
                            //TODO show error to user
                        }
                    }
                }
                else
                {
                    //the user forgot to put in a user
                    //confusing huh

                    //TODO show error to user
                }
            }

            return userST;
        }


        /// <summary>
        /// This function gathers all of the Unique permissions acrros all sub sites, lists and items from the current site and compiles a list of permissions 
        /// using the UserData Data Model
        /// </summary>
        /// <param name="myweb">This is the current website the user is on</param>
        /// <param name="users">this is a list of the (two) users selected in the two comparing people pickers</param>
        /// <returns>The list returned is a list of all (both) users selected in the two comparing people pickers with a list of all unique permissions</returns>
        private UserData[] checkingUsers(SPWeb myweb, UserData[] users)
        {
            SPWebApplication webApp = SPContext.Current.Site.WebApplication;
            SPSite site = SPContext.Current.Site;
            var allUnique = myweb.GetWebsAndListsWithUniquePermissions();

            foreach (SPWebListInfo WLI in allUnique)
            {
                SPObjectType objType = WLI.Type;
                Type webType = typeof(SPWeb);
                Type listType = typeof(SPList);
                if (objType.ToString() == "Web")
                {
                    SPWeb web = site.OpenWeb(WLI.WebId);
                    foreach (UserData user in users)
                    {
                        if (user != null)
                        {
                            List<userPermissions> userPermsList = checkUserPerms(web, user);
                            foreach (userPermissions userPerms in userPermsList)
                            {
                                if (userPerms.hasPerms)
                                {
                                    user.userPerms.Add(userPerms);
                                }
                            }
                        }
                    }
                }

                else if (objType.ToString() == "List")
                {
                    try
                    {
                        SPList list = myweb.Lists[WLI.ListId];
                        foreach (UserData user in users)
                        {
                            if (user != null)
                            {
                                List<userPermissions> userPermsList = checkUserPerms(list, user);
                                foreach (userPermissions userPerms in userPermsList)
                                {
                                    if (userPerms.hasPerms)
                                    {
                                        user.userPerms.Add(userPerms);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            foreach (SPList list in myweb.Lists)
            {
                var allUniqueItems = list.GetItemsWithUniquePermissions();
                foreach (UserData user in users)
                {
                    if (user != null)
                    {
                        if (allUniqueItems != null)
                        {
                            foreach (SPListItemInfo LIF in allUniqueItems)
                            {
                                SPListItem item = list.GetItemById(LIF.Id);
                                List<userPermissions> itemUpList = checkUserPerms(item, user);
                                foreach (userPermissions itemUp in itemUpList)
                                {
                                    if (itemUp.hasPerms)
                                    {
                                        user.userPerms.Add(itemUp);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            MILCompareUpdPnl.Update();
            return users;
        }
        /// <summary>
        /// This function checks to see if the user passed in has permissions on the passed in web site
        /// </summary>
        /// <param name="web">sharepoint website being checked if user has permissons</param>
        /// <param name="user">User passed in to check if it has permissions </param>
        /// <returns>the permissions returned are the permissions gathered for the user on the sharepoint website</returns>
        public List<userPermissions> checkUserPerms(SPWeb web, UserData user)
        {
            List<userPermissions> upList = new List<userPermissions>();
            
            SPPermissionInfo info = web.GetUserEffectivePermissionInfo(user.userAcct.LoginName);
            foreach (SPRoleAssignment role in info.RoleAssignments)
            {
                userPermissions newUP = new userPermissions(web, null);
                foreach (SPRoleDefinition roleDef in role.RoleDefinitionBindings)
                {
                    if (newUP.hasPerms == false)
                    {
                        newUP.hasPerms = true;
                    }
                    newUP.roleDef.Add(roleDef);
                    newUP.roleAssignment = role;
                    if (role.Member is SPGroup)
                    {
                        SPGroup group = (SPGroup)role.Member;
                        newUP.isGroup = true;

                    }
                    else
                    {
                        SPUser user2 = (SPUser)role.Member;
                        if (user2.IsDomainGroup)
                        {
                            newUP.isADGroup = true;
                        }
                    }
                    break;

                }
                //break;
                upList.Add(newUP);
            }
            return upList;
        }
        /// <summary>
        /// This function checks to see if the user passed in has permissions on the passed in sharepoint List
        /// </summary>
        /// <param name="list">sharepoint list being checked if user has permissons</param>
        /// <param name="user">User passed in to check if it has permissions </param>
        /// <returns>the permissions returned are the permissions gathered for the user on the sharepoint List</returns>
        public List<userPermissions> checkUserPerms(SPList list, UserData user)
        {
            List<userPermissions> upList = new List<userPermissions>();
            SPPermissionInfo info = list.GetUserEffectivePermissionInfo(user.userAcct.LoginName);
            foreach (SPRoleAssignment role in info.RoleAssignments)
            {

                userPermissions newUP = new userPermissions(list, null);
                foreach (SPRoleDefinition roleDef in role.RoleDefinitionBindings)
                {
                    if (newUP.hasPerms == false)
                    {
                        newUP.hasPerms = true;
                    }
                    newUP.roleDef.Add(roleDef);
                    newUP.roleAssignment = role;
                    if (role.Member is SPGroup)
                    {
                        SPGroup group = (SPGroup)role.Member;
                        newUP.isGroup = true;

                    }
                    else
                    {
                        SPUser user2 = (SPUser)role.Member;
                        if (user2.IsDomainGroup)
                        {
                            newUP.isADGroup = true;
                        }
                    }
                    break;

                }
                upList.Add(newUP);
            }
            return upList;
        }
        /// <summary>
        /// This function checks to see if the user passed in has permissions on the passed in sharepoint Item
        /// </summary>
        /// <param name="item">sharepoint item being checked if user has permissons</param>
        /// <param name="user">User passed in to check if it has permissions </param>
        /// <returns>the permissions returned are the permissions gathered for the user on the currently selected sharepoint Item</returns>
        public List<userPermissions> checkUserPerms(SPListItem item, UserData user)
        {
            List<userPermissions> upList = new List<userPermissions>();
            SPPermissionInfo info = item.GetUserEffectivePermissionInfo(user.userAcct.LoginName);
            foreach (SPRoleAssignment role in info.RoleAssignments)
            {
                userPermissions newUP = new userPermissions(item, null);
                foreach (SPRoleDefinition roleDef in role.RoleDefinitionBindings)
                {
                    if (newUP.hasPerms == false)
                    {
                        newUP.hasPerms = true;
                    }
                    newUP.roleDef.Add(roleDef);
                    newUP.roleAssignment = role;
                    if (role.Member is SPGroup)
                    {
                        SPGroup group = (SPGroup)role.Member;
                        newUP.isGroup = true;

                    }
                    else
                    {
                        SPUser user2 = (SPUser)role.Member;
                        if (user2.IsDomainGroup)
                        {
                            newUP.isADGroup = true;
                        }
                    }
                   break;

                }
                upList.Add(newUP);
            }
            return upList;
        }
        #endregion
    }
}

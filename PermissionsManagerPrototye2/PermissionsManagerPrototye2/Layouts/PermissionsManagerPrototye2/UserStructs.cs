using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint;

namespace PermissionsManagerPrototye2.Layouts.PermissionsManagerPrototye2
{
    public class UserData
    {
        public SPUser userAcct = null;
        public List<userPermissions> userPerms = null;
        public SPGroupCollection groupCol = null;

        public UserData(SPUser newUser)
        {
            userAcct = newUser;
            userPerms = new List<userPermissions>();
            groupCol = null;
        }


    }
    public class userPermissions
    {
        //still not sure about data representation here, Should i store an object or the object name, could be allot less data but allot more computing time
        //trying to find the correct web/list/item BUT in this current state, it is a lot more storage to store the site for each permissions
        //another thing we could do is have a list all the different roles for each site/list/item which would still allow us to reference the object but not store it 1000 times
        public String objectType = null;
        public SPList list = null;
        public SPWeb web = null;
        public SPListItem item = null;
        public List<SPRoleDefinition> roleDef = null;
        public bool hasPerms = false;
        public bool isGroup = false;
        public bool isADGroup = false;
        public SPRoleAssignment roleAssignment = null;
        public userPermissions()
        {
            roleDef = new List<SPRoleDefinition>();
        }
        public userPermissions(SPWeb newWeb, SPRoleDefinition newRoleDef)
        {
            objectType = "Web";
            web = newWeb;
            roleDef = new List<SPRoleDefinition>();
            if (newRoleDef != null)
            {
                roleDef.Add(newRoleDef);
            }
        }
        public userPermissions(SPList newList, SPRoleDefinition newRoleDef)
        {
            objectType = "List";
            list = newList;
            roleDef = new List<SPRoleDefinition>();
            if (newRoleDef != null)
            {
                roleDef.Add(newRoleDef);
            }
        }
        public userPermissions(SPListItem newItem, SPRoleDefinition newRoleDef)
        {
            objectType = "Item";
            item = newItem;
            roleDef = new List<SPRoleDefinition>();
            if (newRoleDef != null)
            {
                roleDef.Add(newRoleDef);
            }
        }
    }
    [Serializable]
    public class TransactionCollection
    {
        public List<Transaction> transactionList;
        public TransactionCollection()
        {
            transactionList = new List<Transaction>();
        }
        public void Add(SPUser user, SPWeb web, String roleDef)
        {
            Transaction tr = new Transaction(user, web, roleDef);
            transactionList.Add(tr);
        }
        public void Add(SPUser user, SPList list, String roleDef)
        {
            Transaction tr = new Transaction(user, list, roleDef);
            transactionList.Add(tr);
        }
        public void Add(SPUser user, SPGroup group, String roleDef)
        {
            Transaction tr = new Transaction(user, group, roleDef);
            transactionList.Add(tr);
        }
    }
    [Serializable]
    public class Transaction
    {
        //public SPUser user;
        public int userID;
        //protected List<SPRoleDefinition> roleDef;
        //public SPRoleAssignment roleAssignment;
        public String roleDefName;
        public String objectType;
        public String objectName;
        public String spObject;
        public String parentURL;
        //public SPWeb web;
        //public SPList list;
        //public SPGroup group;
        public Transaction(SPUser user, SPWeb web, String roleDef)
        {
            //this.user = user;
            this.objectType = "Web";
            this.userID = user.ID;
            //this.web = web;
            this.spObject = web.Url;
            //this.parentURL = web.ParentWeb.Url;
            this.objectName = web.Title;
            //this.roleAssignment = roleAssignment;
            this.roleDefName = roleDef;
        }
        public Transaction(SPUser user, SPList list, String roleDef)
        {
            //this.user = user;
            this.userID = user.ID;
            this.objectType = "List";
            //this.list = list;
            this.spObject = list.Title;
            this.parentURL = list.ParentWeb.Url;
            this.objectName = list.Title;
            this.roleDefName = roleDef;
        }
        public Transaction(SPUser user, SPGroup group, String roleDef)
        {
            //this.user = user;
            this.userID = user.ID;
            this.objectType = "Group";
            //this.group = group;
            this.spObject = group.Name;
            this.parentURL = group.ParentWeb.Url;
            this.objectName = group.Name;
            this.roleDefName = roleDef;

        }

    }
}

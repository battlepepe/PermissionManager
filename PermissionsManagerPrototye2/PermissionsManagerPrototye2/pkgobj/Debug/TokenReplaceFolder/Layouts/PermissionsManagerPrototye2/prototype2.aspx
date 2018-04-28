<%@ Assembly Name="PermissionsManagerPrototye2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=932ac741e911d235" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="prototype2.aspx.cs" Inherits="PermissionsManagerPrototye2.Layouts.PermissionsManagerPrototye2.prototype2" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">
     <style type="text/css">
        .inlineBlock {
            display: inline-block;
            border: solid;
            margin-left: 0%;
            margin-right: 2%;
        }

        .top {
            min-height: 10%;
        }

        .leftSide {
            display: inline-block;
            border: solid;
            min-height: 150px;
            height: 20%;
            min-width: 25%;
            width: 25%;
            float: left;
            margin-left: 0%;
            margin-right: 2%;
        }

        .rightSide {
            display: inline-block;
            border: solid;
            width: 65%;
            max-width: 65%;
            position:relative;
            left:25%;
            margin-left: 0%;
            margin-right: 2%;
        }

        .instruct {
            font-size: medium;
        }

        .exit {
            display: inline-block;
            margin-left: 88%;
        }

        .loading {
            width: 50px;
            height: 50px;
        }
    </style>
</asp:Content>


<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
    <asp:Panel ID="Top" runat="server">
        <asp:Button ID="exitBtn" CssClass="exit" Text="X" runat="server" />
    </asp:Panel>
    <asp:Panel runat="server">
        <p id="MILInstr" class="instruct">Permissions Manager</p>
    </asp:Panel>
    <!-- ******************************************LEFT SIDE*******************************************************-->
    <asp:Panel ID="MILLeftSide" CssClass="leftSide" runat="server">
        <asp:DropDownList ID="MILactionDD" runat="server">
            <asp:ListItem Value="PPA" Text="Please Pick Another" />
            <asp:ListItem Value="Adding" Text="Adding" />
            <asp:ListItem Value="Removing" Text="Removing" />
            <asp:ListItem Value="Comparing" Text="Comparing" />
            <asp:ListItem Value="Replacing" Text="replacing" />
        </asp:DropDownList>


        <asp:UpdatePanel ID="MILSpecificsUpdPnl" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
            <ContentTemplate>
                <asp:DropDownList ID="MILtypeDD" runat="server" OnSelectedIndexChanged="loadTable" AutoPostBack="true" Style="display: none;">
                    <asp:ListItem Value="PPA" Text="Please Pick Another" />
                    <asp:ListItem Value="Sites" Text="Sites" />
                    <asp:ListItem Value="Lists" Text="Lists" />
                    <asp:ListItem Value="Groups" Text="Groups" />
                </asp:DropDownList>

                <SharePoint:PeopleEditor ID="MILPPicker" runat="server" MultiSelect="true" MaximumEntities="10" Style="display: none;" />

                
                <asp:UpdatePanel runat="server" ID="MILObjectUpdPnl" ChildrenAsTriggers="false" UpdateMode="Conditional">
                    <ContentTemplate>
                    <asp:Panel ID="MILObjectPnl" runat="server">
                            <asp:CheckBoxList ID="MILobjectList" runat="server" Style="display: none;">
                            </asp:CheckBoxList>
                        </asp:Panel>
                    </ContentTemplate>
                </asp:UpdatePanel>

                <asp:Button ID="MILselectItemsBtn" Text="Select" runat="server" Style="display: none;" />
                <asp:Button ID="MILCompareBtn" runat="server" Text="Compare Time" OnClick="compareUsers" Style="display: none;" />
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Panel>



    <!-- ******************************************RIGHT SIDE*******************************************************-->
    <asp:Panel CssClass="MILRightSide" runat="server">
        <asp:UpdateProgress ID="MILUpdProgress" runat="server">
            <ProgressTemplate>
                <asp:Image ID="imgRightWaitIcon" runat="server" ImageAlign="AbsMiddle" ImageUrl="../images/SPINNYREFRESH.GIF" />
                Processing...
            </ProgressTemplate>
        </asp:UpdateProgress>


        <asp:Panel runat="server" ID="MILAddPnl" style="display:none;">
            <asp:UpdatePanel ID="MILAddUpdPnl" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:Label ID="MILAddingUserLabel" Text="permissions to add" runat="server" />                    
                    <asp:Panel ID="MILAddPermsPanel" runat="server">
                    <asp:ListBox runat="server" ID="MILAddPermListBox" Rows="10" SelectionMode="Multiple"></asp:ListBox>
                    </asp:Panel>

                    <asp:Button ID="MILAddPerms" Text="Select" runat="server" OnClick="AddPermissions" />
                    <asp:Table ID="MILAddingTransaction" runat="server" BorderStyle="Solid" GridLines="Both">
                       <asp:TableHeaderRow>
                            <asp:TableHeaderCell Text="User"/>
                            <asp:TableHeaderCell Text="Removing from"/>
                        </asp:TableHeaderRow> 
                    </asp:Table>
                    <asp:Button ID="MILConfirmAddingPermsButton" Text ="Confirm Transaction" runat="server" OnClick="CompleteAddTransaction" />
                    <br />
                    <asp:Label ID="MILAddErrorLabel" runat="server"/>
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:Panel>


        <asp:Panel ID="MILRemovePnl" runat="server" style="display:none;">
            <asp:Label ID="MILRemoveLbl" Text="Removing" runat="server" />
            <asp:UpdatePanel ID="MILRemoveUpdPnl" runat="server">
                <ContentTemplate>
                    <asp:ListBox ID="MILRemoveListBox" Rows="10" SelectionMode="Multiple" runat="server"></asp:ListBox>
                    <asp:Panel ID="MILRemovingLblPanel" runat="server">
                    </asp:Panel>
                     <asp:Button ID="MILRemoveBtn" Text="remove" runat="server" OnClick="removePermissions" />
                    <asp:Table ID="MILRemovingTable" GridLines="Both" BorderStyle="Solid" runat="server">
                        <asp:TableHeaderRow>
                            <asp:TableHeaderCell Text="User"/>
                            <asp:TableHeaderCell Text="Removing from"/>
                        </asp:TableHeaderRow> 
                    </asp:Table>
                    
                    <asp:Button ID="MILConfirmRemovingPermsButton" Text ="Confirm Transaction" runat="server" OnClick="CompleteRemoveTransaction" />   
                    <br />
                    <asp:Label ID="MILRemoveErrorLabel" runat="server"/>
                </ContentTemplate>
            </asp:UpdatePanel>
    </asp:Panel>


        <asp:Panel runat="server" ID="MILComparePnl" style="display:none;">
            <asp:UpdatePanel ID="MILCompareUpdPnl" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
                <ContentTemplate>

                    <asp:Label ID="MILCompareMsg" Text="Comparing" runat="server" />
                    <br />
                    <asp:Label ID="MILCompareErrorLabel" runat="server" />
                    <asp:Table ID="MILCompareTable" runat="server" BorderStyle="Solid" GridLines="Both">
                    </asp:Table>

                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:Panel>


        <asp:Panel ID="MILReplacePnl" runat="server" style="display:none;">
            <asp:UpdatePanel ID="MILReplaceUpdPnl" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
                <ContentTemplate>

                    <asp:Label ID="MILReplaceMsg" runat="server" />
                    <br />
                    <asp:Label runat="server" Text="User to be replaced" />
                    <SharePoint:PeopleEditor ID="MILReplaceOldUserPP" runat="server" MultiSelect="false" MaximumEntities="1" Style="display: block;" />
                    <br />

                    <asp:Label runat="server" Text="User replacing the other" />
                    <SharePoint:PeopleEditor ID="MILReplaceNewUserPP" runat="server" MultiSelect="false" MaximumEntities="1" Style="display: block;" />

                    <asp:Table ID="MILReplaceTable" runat="server" BorderStyle="Solid" GridLines="Both">
                    </asp:Table>
                    <asp:Button ID="MILReplaceShowData" runat="server" Text="get data" Style="display: block;" OnClick="showAllData" />
                    
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:Button ID="MILReplaceBtn" runat="server" Text="get replacing" style="display:none;" OnClick="replaceUsers" />
        </asp:Panel>
    </asp:Panel>
       <script type="text/javascript">

        //0 = MILactionDD
        //1 = MILtypeDD
        //2 = MILPPicker
        //3 = MILObjectPanel
        //4 = MILselectItemsBtn
        //5 = MILCompareBtn

        var elements = ["<%=MILactionDD.ClientID%>", "<%=MILtypeDD.ClientID%>",
                        "<%=MILPPicker.ClientID%>", "<%=MILObjectPnl.ClientID%>",
                        "<%=MILselectItemsBtn.ClientID%>", "<%=MILCompareBtn.ClientID%>"];

        function changeAction() {
            var actionElement = document.getElementById("<%=MILactionDD.ClientID%>");
            var instr = document.getElementById("MILInstr");

            //hide everthing 
            hideElements([1, 2, 3, 4, 5]);
            document.getElementById("<%= MILAddPnl.ClientID%>").style.display = "none";
            document.getElementById("<%= MILRemovePnl.ClientID%>").style.display = "none";
            document.getElementById("<%= MILComparePnl.ClientID%>").style.display = "none";
            document.getElementById("<%= MILReplacePnl.ClientID%>").style.display = "none";
            document.getElementById("<%= MILReplaceBtn.ClientID%>").style.display = "none";

            //show only things they need
            switch (actionElement.value) {
                case "Adding":
                    instr.innerHTML = "Here is where you can add one or more people to one or more lists/sites/pages";
                    showElements([1]);
                    break;
                case "Removing":
                    instr.innerHTML = "Here is where you can remove one person from one or more sites/lists/pages/groups";
                    showElements([1]);
                    break;

                case "Replacing":
                    instr.innerHTML = "Here is where you can replace one person with another in the event someone leaves";
                    showElements([4]);
                    break;
                case "Comparing":
                    instr.innerHTML = "Here is where you can compare multiple people or groups to one another(maximum 2)";
                    showElements([2, 5]);
                    break;
                default:
            }
        }

        function changeObjectList() {
            hideElements([5]);
            showElements([2, 3, 4]);
        }


        //smart function that does smart things
        function hideElements(args) {
            for (var opt in args) {
                if (document.getElementById(elements[args[opt]]).style.display != "none")
                    document.getElementById(elements[args[opt]]).style.display = "none";
            }
        }
        function showElements(args) {
            for (var opt in args) {
                if (document.getElementById(elements[args[opt]]).style.display != "block")
                    document.getElementById(elements[args[opt]]).style.display = "block";
            }
        }



        function changeItem() {
            var actionElement = document.getElementById("<%=MILactionDD.ClientID%>");

            document.getElementById("<%= MILAddPnl.ClientID%>").style.display = "none";
            document.getElementById("<%= MILRemovePnl.ClientID%>").style.display = "none";
            document.getElementById("<%= MILComparePnl.ClientID%>").style.display = "none";
            document.getElementById("<%= MILReplacePnl.ClientID%>").style.display = "none";
            document.getElementById("<%= MILReplaceBtn.ClientID%>").style.display = "none";


            if (actionElement.value == "Adding") {
                document.getElementById("<%= MILAddPnl.ClientID%>").style.display = "block";
            }
            else if (actionElement.value == "Removing") {
                document.getElementById("<%= MILRemovePnl.ClientID%>").style.display = "block";
            }
            else if (actionElement.value == "Comparing") {
                document.getElementById("<%= MILComparePnl.ClientID%>").style.display = "block";
            }
            else if (actionElement.value == "Replacing") {
                document.getElementById("<%= MILReplacePnl.ClientID%>").style.display = "block";
                document.getElementById("<%= MILReplaceBtn.ClientID%>").style.display = "block";
            }


}

    </script>
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
    Application Page
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server">
    My Application Page
    
</asp:Content>

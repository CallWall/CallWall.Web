﻿@model $rootnamespace$.Models.OAuthAccountListItem

@using (Html.BeginForm("Authenticate", "Account"))
{
    @Html.AntiForgeryToken()
    <input type="hidden" name="account" value="@Model.Name"/>
    <fieldset>
        <legend>
            <img src="@Model.Image" height="100" alt="@Model.Name" class="account-icon"/>
        </legend>
        @Html.EditorFor(m => m.Resources)
        <input type="submit" value="@string.Format("{0}", Model.IsActive ? "Registered":"Register")" style="float: right" @string.Format("{0}", Model.IsActive ? "disabled='disabled'":"")/>
    </fieldset>
}




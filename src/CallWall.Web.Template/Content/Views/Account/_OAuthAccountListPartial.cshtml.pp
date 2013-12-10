@model IEnumerable<$rootnamespace$.Models.OAuthAccountListItem>

@foreach (var accountConfig in Model)
{
  @Html.DisplayFor(m => accountConfig, "AccountConfiguration")
} 

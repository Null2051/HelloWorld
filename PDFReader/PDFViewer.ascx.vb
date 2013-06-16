﻿Imports System.Text.RegularExpressions
Imports System.Drawing.Imaging
Imports System.Drawing
Imports System.Windows.Forms
Imports PDFLibNet
Imports System.ComponentModel

Partial Public Class WebUserControl1
  Inherits System.Web.UI.UserControl

#Region "Private Declarations"
  Private parameterHash As Hashtable
  Private panelHeightFactor As Single = 0.9
  Private panelWidthFactor As Single = 0.73
  Private panelBookWidthFactor As Single = 0.24
  Private zoomFactor As Single = 1.25
  Private minDPI As Integer = 20
  Private maxDPI As Integer = 400
  Private baseDPI As Integer = 150
#End Region

#Region "Properties"

  Public Property FileName() As String
    Get
      Return parameterHash("PDFFileName")
    End Get
    Set(ByVal value As String)
      If Nothing = value Or value = "" Then
        Me.Enabled = False
        Exit Property
      End If
      If ImageUtil.IsPDF(value) Then
        Me.Enabled = True
        InitUserVariables()
        parameterHash("PDFFileName") = value
        InitPageRange()
        InitRotation()
        parameterHash("PagesOnly") = False
        InitBookmarks()
        FitToWidthButton_Click(Nothing, Nothing)
      End If
    End Set
  End Property

  Public WriteOnly Property Enabled()
    Set(ByVal value)
      MainPanel.Enabled = value
    End Set
  End Property

#End Region

#Region "Control based events"

  'Need to delete the last image of the page viewed

  Private Sub Page_Init(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Init
    'Persist User control state
    Page.RegisterRequiresControlState(Me)
  End Sub

  Protected Overrides Function SaveControlState() As Object
    Return parameterHash
  End Function

  Protected Overrides Sub LoadControlState(ByVal savedState As Object)
    parameterHash = CType(savedState, Hashtable)
  End Sub


  Protected Sub Control_load() Handles MyBase.Load
    ResizePanels()
    If Not Nothing Is parameterHash Then
      parameterHash("SearchText") = SearchTextBox.Text
    End If
    PreviousPageButton.Attributes.Add("onclick", "getBrowserDimensions()")
    NextPageButton.Attributes.Add("onclick", "getBrowserDimensions()")
    ZoomInButton.Attributes.Add("onclick", "getBrowserDimensions()")
    ZoomOutButton.Attributes.Add("onclick", "getBrowserDimensions()")
    RotateCCButton.Attributes.Add("onclick", "getBrowserDimensions()")
    RotateCButton.Attributes.Add("onclick", "getBrowserDimensions()")

  End Sub

  Protected Sub hiddenPageNav_Click(ByVal sender As Object, ByVal e As EventArgs) Handles HiddenPageNav.Click
    parameterHash("CurrentPageNumber") = HiddenPageNumber.Value
    DisplayCurrentPage()
  End Sub

  Protected Sub PageNumberTextBox_TextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles PageNumberTextBox.TextChanged
    If Regex.IsMatch(PageNumberTextBox.Text, "^\d+$") Then
      parameterHash("CurrentPageNumber") = PageNumberTextBox.Text
      DisplayCurrentPage()
    End If
  End Sub

  Protected Sub PreviousPageButton_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles PreviousPageButton.Click
    parameterHash("CurrentPageNumber") -= 1
    DisplayCurrentPage()
  End Sub

  Protected Sub NextPageButton_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles NextPageButton.Click
    parameterHash("CurrentPageNumber") += 1
    DisplayCurrentPage()
  End Sub

  Protected Sub ZoomOutButton_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ZoomOutButton.Click
    parameterHash("DPI") /= zoomFactor
    If parameterHash("DPI") < minDPI Then
      parameterHash("DPI") = minDPI
    End If
    DisplayCurrentPage()
  End Sub

  Protected Sub ZoomInButton_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ZoomInButton.Click
    parameterHash("DPI") *= zoomFactor
    If parameterHash("DPI") > maxDPI Then
      parameterHash("DPI") = maxDPI
    End If
    DisplayCurrentPage()
  End Sub

  Protected Sub RotateCCButton_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles RotateCCButton.Click
    Dim indexNum As Integer = (parameterHash("CurrentPageNumber") - 1)
    parameterHash("RotationPage")(indexNum) -= 1
    DisplayCurrentPage()
  End Sub

  Protected Sub RotateCButton_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles RotateCButton.Click
    Dim indexNum As Integer = (parameterHash("CurrentPageNumber") - 1)
    parameterHash("RotationPage")(indexNum) += 1
    DisplayCurrentPage()
  End Sub

#End Region

#Region "Constraints"

  Private Sub CheckPageBounds()

    Dim pageCount As Integer = parameterHash("PDFPageCount")

    If parameterHash("CurrentPageNumber") >= pageCount Then
      parameterHash("CurrentPageNumber") = pageCount
      NextPageButton.Enabled = False
    ElseIf parameterHash("CurrentPageNumber") <= 1 Then
      parameterHash("CurrentPageNumber") = 1
      PreviousPageButton.Enabled = False
    End If

    If parameterHash("CurrentPageNumber") < pageCount And pageCount > 1 And parameterHash("CurrentPageNumber") > 1 Then
      NextPageButton.Enabled = True
      PreviousPageButton.Enabled = True
    End If

    If parameterHash("CurrentPageNumber") = pageCount And pageCount > 1 And parameterHash("CurrentPageNumber") > 1 Then
      PreviousPageButton.Enabled = True
    End If

    If parameterHash("CurrentPageNumber") = 1 And pageCount > 1 Then
      NextPageButton.Enabled = True
    End If

    If pageCount = 1 Then
      NextPageButton.Enabled = False
      PreviousPageButton.Enabled = False
    End If

  End Sub

#End Region

#Region "Helper Functions"

  Private Sub InitUserVariables()
    parameterHash = New Hashtable
    parameterHash.Add("PDFFileName", "")
    parameterHash.Add("PDFPageCount", 0)
    parameterHash.Add("CurrentPageNumber", 1)
    parameterHash.Add("UserPassword", "")
    parameterHash.Add("OwnerPassword", "")
    parameterHash.Add("Password", "")
    parameterHash.Add("DPI", baseDPI)
    parameterHash.Add("PagesOnly", False)
    parameterHash.Add("CurrentImageFileName", "")
    parameterHash.Add("Rotation", New List(Of Integer))
    parameterHash.Add("Bookmarks", "")
    parameterHash.Add("SearchText", "")
    parameterHash.Add("SearchDirection", AFPDFLibUtil.SearchDirection.FromBeginning)
    parameterHash.Add("UseXPDF", True)
  End Sub

  Private Sub UpdatePageLabel()
    PageLabel.Text = "Page " & parameterHash("CurrentPageNumber") & " of " & parameterHash("PDFPageCount")
    PageNumberTextBox.Text = parameterHash("CurrentPageNumber")
  End Sub

  Private Sub InitPageRange()
    parameterHash("PDFPageCount") = ImageUtil.GetImageFrameCount(parameterHash("PDFFileName"), parameterHash("Password"))
    parameterHash("CurrentPageNumber") = 1
  End Sub

  Private Sub InitBookmarks()
    Dim pdfDoc As PDFLibNet.PDFWrapper
    Try
      pdfDoc = New PDFLibNet.PDFWrapper
      pdfDoc.LoadPDF(parameterHash("PDFFileName"))
    Catch ex As Exception
      'pdfDoc failed
      If Not Nothing Is pdfDoc Then
        pdfDoc.Dispose()
      End If
    End Try
    Dim bookmarkHtml As String
    If Not Nothing Is pdfDoc Then
      bookmarkHtml = AFPDFLibUtil.BuildHTMLBookmarks(pdfDoc, parameterHash("PagesOnly"))
      pdfDoc.Dispose()
    End If
    BookmarkContentCell.Text = bookmarkHtml
    If Regex.IsMatch(bookmarkHtml, "\<\!--PageNumberOnly--\>") Then
      parameterHash("PagesOnly") = True
    End If
  End Sub

  Private Sub InitRotation()
    parameterHash("RotationPage") = New List(Of Integer)
    For i As Integer = 1 To parameterHash("PDFPageCount")
      CType(parameterHash("RotationPage"), List(Of Integer)).Add(0)
    Next
  End Sub

  Private Sub ResizePanels()
    If HiddenBrowserWidth.Value <> "" And HiddenBrowserHeight.Value <> "" Then
      BookmarkPanel.Width = HiddenBrowserWidth.Value * panelBookWidthFactor
      BookmarkPanel.Height = HiddenBrowserHeight.Value * panelHeightFactor
      ImagePanel.Width = HiddenBrowserWidth.Value * panelWidthFactor
      ImagePanel.Height = HiddenBrowserHeight.Value * panelHeightFactor
    End If
  End Sub

  Private Sub DisplayCurrentPage(Optional ByVal doSearch As Boolean = False)
    'Set how long to wait before deleting the generated PNG file
    Dim expirationDate As DateTime = Now.AddMinutes(5)
    Dim noSlide As TimeSpan = System.Web.Caching.Cache.NoSlidingExpiration
    Dim callBack As New CacheItemRemovedCallback(AddressOf OnCacheRemove)
    ResizePanels()
    CheckPageBounds()
    UpdatePageLabel()
    InitBookmarks()
    Dim destPath As String = Request.MapPath("render")
    Dim indexNum As Integer = (parameterHash("CurrentPageNumber") - 1)
    Dim numRotation As Integer = parameterHash("RotationPage")(indexNum)
    Dim imageLocation As String
    If doSearch = False Then
      imageLocation = ASPPDFLib.GetPageFromPDF(parameterHash("PDFFileName"), destPath, parameterHash("CurrentPageNumber"), parameterHash("DPI"), parameterHash("Password"), numRotation)
    Else
      imageLocation = ASPPDFLib.GetPageFromPDF(parameterHash("PDFFileName"), destPath _
                                               , parameterHash("CurrentPageNumber") _
                                               , parameterHash("DPI") _
                                               , parameterHash("Password") _
                                               , numRotation, parameterHash("SearchText") _
                                               , parameterHash("SearchDirection") _
                                               )
      UpdatePageLabel()
    End If
    ImageUtil.DeleteFile(parameterHash("CurrentImageFileName"))
    parameterHash("CurrentImageFileName") = imageLocation
    'Add full filename to the Cache with an expiration
    'When the expiration occurs, it will call OnCacheRemove whih will delete the file
    Cache.Insert(New Guid().ToString & "_DeleteFile", imageLocation, Nothing, expirationDate, noSlide, System.Web.Caching.CacheItemPriority.Default, callBack)
    Dim matchString As String = Request.MapPath("").Replace("\", "\\") ' escape backslashes
    CurrentPageImage.ImageUrl = Regex.Replace(imageLocation, matchString & "\\", "~/")
  End Sub

  Private Sub OnCacheRemove(ByVal key As String, ByVal val As Object, ByVal reason As CacheItemRemovedReason)
    If Regex.IsMatch(key, "DeleteFile") Then
      ImageUtil.DeleteFile(val)
    End If
  End Sub

#End Region

  Protected Sub FitToScreenButton_Click(ByVal sender As Object, ByVal e As EventArgs) Handles FitToScreenButton.Click
    Dim panelsize As Drawing.Size = New Size(HiddenBrowserWidth.Value * panelWidthFactor, HiddenBrowserHeight.Value * panelHeightFactor)
    parameterHash("DPI") = AFPDFLibUtil.GetOptimalDPI(parameterHash("PDFFileName"), parameterHash("CurrentPageNumber"), panelsize)
    DisplayCurrentPage()
  End Sub

  Protected Sub FitToWidthButton_Click(ByVal sender As Object, ByVal e As EventArgs) Handles FitToWidthButton.Click
    Dim panelsize As Drawing.Size = New Size(HiddenBrowserWidth.Value * panelWidthFactor, HiddenBrowserHeight.Value * 4)
    parameterHash("DPI") = AFPDFLibUtil.GetOptimalDPI(parameterHash("PDFFileName"), parameterHash("CurrentPageNumber"), panelsize)
    DisplayCurrentPage()
  End Sub

  Protected Sub ActualSizeButton_Click(ByVal sender As Object, ByVal e As EventArgs) Handles ActualSizeButton.Click
    parameterHash("DPI") = 150
    DisplayCurrentPage()
  End Sub

  Protected Sub SearchButton_Click(ByVal sender As Object, ByVal e As EventArgs) Handles SearchButton.Click
    parameterHash("SearchDirection") = AFPDFLibUtil.SearchDirection.FromBeginning
    DisplayCurrentPage(True)
    'BookmarkContentCell.Text = AFPDFLibUtil.BuildHTMLBookmarksFromSearchResults( _
    'AFPDFLibUtil.GetAllSearchResults(parameterHash("PDFFileName"), parameterHash("SearchText")) _
    ')
  End Sub

  Protected Sub SearchNextButton_Click(ByVal sender As Object, ByVal e As EventArgs) Handles SearchNextButton.Click
    parameterHash("SearchDirection") = AFPDFLibUtil.SearchDirection.Forwards
    DisplayCurrentPage(True)
  End Sub

  Protected Sub SearchPreviousButton_Click(ByVal sender As Object, ByVal e As EventArgs) Handles SearchPreviousButton.Click
    parameterHash("SearchDirection") = AFPDFLibUtil.SearchDirection.Backwards
    DisplayCurrentPage(True)
  End Sub
End Class
<?php
/*
 * Created on 2010-11-17
 *
 * To change the template for this generated file go to
 * Window - Preferences - PHPeclipse - PHP - Code Templates
 */
	//获取文件所在目录
	$dir=dirname(__FILE__) ;
	//删除测试文件
//	@unlink( $dir."\\test.swf" );
	//使用pdf2swf转换命令
	$command= "D:/SWFTools/pdf2swf.exe  -t \"".$dir."\\test.pdf\" -o  \"".$dir."\\test.swf\" -s flashversion=9 ";
	//创建shell对象
	$WshShell   = new COM("WScript.Shell");
	//执行cmd命令
	$oExec      = $WshShell->Run("cmd /C ". $command, 0, true);
?>
<html >
    <head>
        <title></title>
        <style type="text/css" media="screen">
			html, body	{ height:100%; }
			body { margin:0; padding:0; overflow:auto; }
			#flashContent { display:none; }
        </style>

		<script type="text/javascript" src="js/swfobject/swfobject.js"></script>
		<script type="text/javascript">
            var swfVersionStr = "10.0.0";
            var xiSwfUrlStr = "playerProductInstall.swf";
            var flashvars = {
                  SwfFile : escape("test.swf"),
				  Scale : 0.6,
				  ZoomTransition : "easeOut",
				  ZoomTime : 0.5,
  				  ZoomInterval : 0.1,
  				  FitPageOnLoad : false,
  				  FitWidthOnLoad : true,
  				  PrintEnabled : true,
  				  FullScreenAsMaxWindow : false,
  				  ProgressiveLoading : true,

  				  PrintToolsVisible : true,
  				  ViewModeToolsVisible : true,
  				  ZoomToolsVisible : true,
  				  FullScreenVisible : true,
  				  NavToolsVisible : true,
  				  CursorToolsVisible : true,
  				  SearchToolsVisible : true,
  				  localeChain: "zh_CN"
				  };

			 var params = {

			    }
            params.quality = "high";
            params.bgcolor = "#ffffff";
            params.allowscriptaccess = "sameDomain";
            params.allowfullscreen = "true";
            var attributes = {};
            attributes.id = "FlexPaperViewer";
            attributes.name = "FlexPaperViewer";
            swfobject.embedSWF(
                "FlexPaperViewer.swf", "flashContent",
                "650", "500",
                swfVersionStr, xiSwfUrlStr,
                flashvars, params, attributes);
			swfobject.createCSS("#flashContent", "display:block;text-align:left;");
        </script>

    </head>
    <body>
    	<div style="position:absolute;left:10px;top:10px;">
	        <div id="flashContent">
	        </div>
        </div>
   </body>
</html>
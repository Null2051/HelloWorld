/**
 * 弹出窗口类
 */
package
{
	import flash.display.DisplayObject;

	import mx.containers.TitleWindow;
	import mx.controls.ProgressBar;
	import mx.events.CloseEvent;
	import mx.managers.PopUpManager;

	public class Dialog
	{
		public function Dialog(target:Object, isModal:Boolean=false)
		{
			_target=target;
			_isModal=isModal;
		}
		private var _target:Object;
		private var _isModal:Boolean=false;
		private var _height:int=100;
		private var _id:int;

		private var _left:String;

		private var _title:String;
		private var _top:String;

		private var _url:String;

		private var _width:int=300;

		/*
		   ID：窗口id号，可省略。每个窗口的id必须是唯一的不能重复。
		   Title：窗口标题。如不写此项默认值为""。
		   URL： 窗口内容页地址，或使用相对路径或绝对路径，注意如果使用http://www.host.com形式的绝对地址，则http://不能省略。
		   InnerHtml： 窗口内容html代码，用于直接输出html内容，注意不要让生成的内容因为不适当的宽度或定位方式而破坏了Dialog的外观。
		   InvokeElementId： 本页面内隐藏的元素的id，用于显示页面内隐藏的元素中的html内容，注意不要让内容因为不适当的宽度或定位方式而破坏了Dialog的外观。
		   Width：窗口宽度（dialog内容区宽度），值为数值型，默认值为窗口可见宽的40%。
		   Height：窗口高度（dialog内容区高度），值为数值型，默认值为窗口可见宽的20%。
		   Left：窗口距浏览器左边距离，值为数值型或字符串型（当使用百分比时为字符串型），如Left:"0%",Top:"0%"为左上，Left:"50%",Top:"50%"为居中，Left:"100%",Top:"100%"为右下。
		   Top：窗口距浏览器顶端距离，值为数值型或字符串型（百分比）。
		   Drag：是否允许拖动窗口，值为布尔型(true|false)，默认值为true，注意需要页面引用了Drag.js。
		   OKEvent：点击确定按钮后执行的函数。
		   CancelEvent：点击取消按钮或点击关闭按钮后执行的函数，默认为关闭本Dialog。
		   ShowButtonRow：是否不显示按钮栏，值为布尔型(true|false)，默认值为false，当定义了OKEvent或调用了addButton时自动设为true。
		   MessageTitle,Message：自定义的窗口说明栏中的小标题和说明。
		   ShowMessageRow：是否显示窗口说明栏，值为布尔型(true|false)，默认值为false，当定义了MessageTitle或Message时自动设为true。
		   AutoClose：是否自行关闭，值为数值型，默认值为false。
		   OnLoad：窗口内容载入完成后执行的程序，值为函数型。
		 */


		private function set ID(value:int):void
		{
			this._id=value;
		}

		private function set Left(value:String):void
		{
			this._left=value;
		}

		private function set Title(value:String):void
		{
			this._title=value;
		}

		private function set Top(value:String):void
		{
			this._top=value;
		}

		private function set URL(value:String):void
		{
			this._url=value;
		}

		public function get Width():int
		{
			return _width;
		}

		public function set Width(value:int):void
		{
			_width=value;
		}


		public function get Height():int
		{
			return _height;
		}

		public function set Height(value:int):void
		{
			_height=value;
		}

		public function get ID():int
		{
			return _id;
		}

		public function get Left():String
		{
			return _left;
		}

		public function get Title():String
		{
			return _title;
		}

		public function get Top():String
		{
			return _top;
		}

		public function get URL():String
		{
			return _url;
		}

		private var _showCloseButton:Boolean=true;

		public function set ShowCloseButton(value:Boolean):void
		{
			this._showCloseButton=value;
		}

		public function defaultCloseHandler(e:CloseEvent):void
		{
			PopUpManager.removePopUp(titleWindow);
		}

		private var titleWindow:TitleWindow;
		private var bar:ProgressBar;

		public function open(title:String="正在处理...", data:Object=null, showCloseButton:Boolean=false, closeHandler:Function=null):void
		{
			this._title=title;
			titleWindow=new TitleWindow();
			titleWindow.title=this._title;
			titleWindow.showCloseButton=true;
			titleWindow.width=this.Width;
			titleWindow.height=this.Height;
			titleWindow.showCloseButton=showCloseButton;

			titleWindow.addEventListener(CloseEvent.CLOSE, defaultCloseHandler);
			if (data != null)
			{

				if (data as DisplayObject)
				{
					titleWindow.addChild(data as DisplayObject);
				}
				else if (getExt(data.toString()) == "swf")
				{

					//Common.loadModule(data.toString(), titleWindow);
					//Common.loadModule(titleWindow, data.toString());
					//Logger.getLogger("Dialog").debug("加载模块....");
				}
				else
				{
					bar=new ProgressBar();
					bar.height=15;
					bar.width=titleWindow.width - 30;
					bar.indeterminate=true;
					bar.label=data.toString();
					titleWindow.addChild(bar);
				}
			}

			PopUpManager.addPopUp(titleWindow, _target as DisplayObject, _isModal);
			PopUpManager.centerPopUp(titleWindow);
		}

		private function getExt(fileName:String):String
		{
			return fileName.substr(fileName.lastIndexOf(".") + 1, 3);
		}

		public function close():void
		{
			PopUpManager.removePopUp(titleWindow);
		}
		
	}
}
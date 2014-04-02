//--- OBJECT WRITE BEGIN ---
new GuiControl(MazeGui)
{
   profile = "GuiDefaultProfile";
   horizSizing = "right";
   vertSizing = "bottom";
   position = "0 0";
   extent = "640 480";
   minExtent = "8 2";
   visible = "1";

   new GuiWindowCtrl()
   {
      profile = "GuiWindowProfile";
      horizSizing = "right";
      vertSizing = "bottom";
      position = "200 115";
      extent = "122 127";
      minExtent = "8 2";
      visible = "1";
      text = "Maze Generator";
      maxLength = "255";
      resizeWidth = "0";
      resizeHeight = "0";
      canMove = "1";
      canClose = "1";
      canMinimize = "0";
      canMaximize = "0";
      minSize = "50 50";
      closeCommand = "canvas.popdialog(MazeGui);";

      new GuiTextCtrl()
      {
         profile = "GuiTextProfile";
         horizSizing = "right";
         vertSizing = "bottom";
         position = "20 32";
         extent = "36 18";
         minExtent = "8 2";
         visible = "1";
         text = "Length:";
         maxLength = "255";
      };
      new GuiTextEditCtrl(MazeHeight)
      {
         profile = "GuiTextEditProfile";
         horizSizing = "right";
         vertSizing = "bottom";
         position = "65 32";
         extent = "35 18";
         minExtent = "8 2";
         visible = "1";
         text = "5";
         maxLength = "255";
         historySize = "0";
         password = "0";
         tabComplete = "0";
         sinkAllKeyEvents = "0";
      };
      new GuiTextCtrl()
      {
         profile = "GuiTextProfile";
         horizSizing = "right";
         vertSizing = "bottom";
         position = "20 55";
         extent = "30 18";
         minExtent = "8 2";
         visible = "1";
         text = "Width:";
         maxLength = "255";
      };
      new GuiTextEditCtrl(MazeWidth)
      {
         profile = "GuiTextEditProfile";
         horizSizing = "right";
         vertSizing = "bottom";
         position = "65 55";
         extent = "35 18";
         minExtent = "8 2";
         visible = "1";
         text = "5";
         maxLength = "255";
         historySize = "0";
         password = "0";
         tabComplete = "0";
         sinkAllKeyEvents = "0";
      };
      new GuiBitmapButtonCtrl()
      {
         profile = "GuiDefaultProfile";
         horizSizing = "right";
         vertSizing = "bottom";
         position = "17 81";
         extent = "88 30";
         minExtent = "8 2";
         visible = "1";
         command = "MazeGui::Generate();";
         text = "        Generate";
         groupNum = "-1";
         buttonType = "PushButton";
         bitmap = "base/client/ui/button1";
         lockAspectRatio = "0";
         alignLeft = "0";
         overflowImage = "0";
         mKeepCached = "0";
         mColor = "255 0 0 255";
      };
   };
};
//--- OBJECT WRITE END ---

$Maze::Colour = 0;
$Maze::AltColour = 4;

if(!$MazeGuiMapped)
{
	$remapDivision[$remapCount] = "Maze Gui";
	$remapName[$remapCount] = "Open Maze Gui";
	$remapCmd[$remapCount] = "ToggleMazeGui";
	$remapCount++;
	$MazeGuiMapped = 1;
}

function ToggleMazeGui(%val)
{
	if(!%val)
		return;
	if(MazeGui.isAwake())
		Canvas.PopDialog(MazeGui);
	else
		Canvas.PushDialog(MazeGui);
}

function MazeGui::Generate(%this)
{
	Canvas.PopDialog(MazeGui);
	%width = MazeWidth.getValue();
	%height = MazeHeight.getValue();
	RandomMaze(%width, %height);
}

function ShowProgress(%done, %total)
{
	%finished = "||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||";
	%len = strlen(%finished);
	%shade = mFloor((%done / %total) * %len);
	%message = "<color:00FF00>" @ getSubStr(%finished, 0, %shade) @ "<color:FF0000>" @ getSubStr(%finished, %shade, %len - %shade);
	clientCmdBottomPrint(%message, -1, 2);
}

function RandomMaze(%w, %h)
{
	%macro = new ScriptObject()
	{
		class = "BuildMacroSO";
		brickArray = "1x1x5\t1x6x5";
		numEvents = 1;
		event0 = "Server\tUseSprayCan" TAB $Maze::AltColour;
	};
	%maze = new ScriptObject()
	{
		class = "MazeSO";
		width = (%w < 1) ? 5 : %w;
		height = (%h < 1) ? 5 : %h;
		macro = %macro;
	};
	for(%y=0;%y<%maze.height;%y++)
		for(%x=0;%x<(%maze.width+1);%x++)
			%maze.vertical[%x, %y] = 1;
	for(%x=0;%x<%maze.width;%x++)
		for(%y=0;%y<(%maze.height+1);%y++)
			%maze.horizontal[%x, %y] = 1;
	for(%y=0;%y<(%maze.width+1);%y++)
	{
		for(%x=0;%x<(%maze.height+1);%x++)
		{
			%macro.addEvent("Server\tPlantBrick");
			if(%x != %maze.height)
				%macro.addEvent("Server\tShiftBrick\t7\t0\t0");
		}
		if(%y != %maze.width)
			%macro.addEvent("Server\tShiftBrick\t" @ (%maze.height * -7) @ "\t7\t0");
	}
	%macro.addEvent("Server\tUseInventory\t1");
	%macro.addEvent("Server\tShiftBrick\t0\t-3\t0");
	%macro.addEvent("Server\tUseSprayCan" TAB $Maze::Colour);
	%maze.x = getRandom(0, (%maze.width-1));
	%maze.y = getRandom(0, (%maze.height-1));
	%maze.visited = 1;
	%maze.stack = 0;
	%maze.cells = %maze.height * %maze.width;
	Schedule(10, 0, "MazeStep", %maze);	
}

function MazeStep(%maze)
{
	%macro = %maze.macro;
	%a = 0;
	if((%maze.x - 1) >= 0 && %maze.vertical[%maze.x-1, %maze.y] && %maze.vertical[%maze.x, %maze.y] && %maze.horizontal[%maze.x-1, %maze.y] && %maze.horizontal[%maze.x-1, %maze.y+1])
	{
		%dirx[%a] = -1;
		%diry[%a] = 0;
		%a++;
	}
	if((%maze.y - 1) >= 0 && %maze.vertical[%maze.x, %maze.y-1] && %maze.vertical[%maze.x+1, %maze.y-1] && %maze.horizontal[%maze.x, %maze.y-1] && %maze.horizontal[%maze.x, %maze.y])
	{
		%dirx[%a] = 0;
		%diry[%a] = -1;
		%a++;
	}
	if((%maze.x + 1) <= (%maze.width-1) && %maze.vertical[%maze.x+1, %maze.y] && %maze.vertical[%maze.x+2, %maze.y] && %maze.horizontal[%maze.x+1, %maze.y] && %maze.horizontal[%maze.x+1, %maze.y+1])
	{
		%dirx[%a] = 1;
		%diry[%a] = 0;
		%a++;
	}
	if((%maze.y + 1) <= (%maze.height-1) && %maze.vertical[%maze.x, %maze.y+1] && %maze.vertical[%maze.x+1, %maze.y+1] && %maze.horizontal[%maze.x, %maze.y+1] && %maze.horizontal[%maze.x, %maze.y+2])
	{
		%dirx[%a] = 0;
		%diry[%a] = 1;
		%a++;
	}
	if(%a > 0)
	{
		%rand = getRandom(0, %a-1);
		%movx = %dirx[%rand];
		%movy = %diry[%rand];
		if(%movx == -1)
			%maze.vertical[%maze.x, %maze.y] = 0;
		else if(%movx == 1)
			%maze.vertical[%maze.x+1, %maze.y] = 0;
		else if(%movy == -1)
			%maze.horizontal[%maze.x, %maze.y] = 0;
		else if(%movy == 1)
			%maze.horizontal[%maze.x, %maze.y+1] = 0;
		%maze.stackx[%maze.stack] = %maze.x;
		%maze.stacky[%maze.stack] = %maze.y;
		%maze.stack++;
		%maze.x += %movx;
		%maze.y += %movy;
		%maze.visited++;
	}
	else
	{
		%maze.stack--;
		%maze.x = %maze.stackx[%maze.stack];
		%maze.y = %maze.stacky[%maze.stack];
	}
	if(%maze.visited < %maze.cells)
	{
		ShowProgress(%maze.visited, %maze.cells);
		Schedule(10, 0, "MazeStep", %maze);
		return;
	}
	%maze.horizontal[0, 0] = 0;
	%maze.horizontal[(%maze.width - 1), %maze.height] = 0;
	for(%y=0;%y<(%maze.height+1);%y++)
	{
		for(%x=0;%x<%maze.width;%x++)
		{
			if(%maze.horizontal[%x, %y] == 1)
				%macro.addEvent("Server\tPlantBrick");
			if(%x != (%maze.width-1))
				%macro.addEvent("Server\tShiftBrick\t0\t-7\t0");
		}
		if(%y != %maze.height)
			%macro.addEvent("Server\tShiftBrick\t-7\t" @ (%maze.width*7)-7 TAB 0);
	}
	%macro.addEvent("Server\tShiftBrick\t" @ ((%maze.height*7)-3) TAB ((%maze.width*7)-4) TAB 0);
	%macro.addEvent("Server\tRotateBrick\t1");
	for(%x=0;%x<(%maze.width+1);%x++)
	{
		for(%y=0;%y<%maze.height;%y++)
		{
			if(%maze.vertical[%x, %y])
				%macro.addEvent("Server\tPlantBrick");
			if(%y != (%maze.height-1))
				%macro.addEvent("Server\tShiftBrick\t-7\t0\t0");
		}
		if(%x != %maze.width)
			%macro.addEvent("Server\tShiftBrick\t" @ (%maze.height*7)-7 @ "\t-7\t0");
	}
	%macro.addEvent("Server\tUseInventory\t0");
	%macro.addEvent("Server\tRotateBrick\t-1");
	%macro.addEvent("Server\tShiftBrick\t-4\t0\t0");
	clientCmdBottomPrint("<color:00FF00>Maze Generated: (" @ %maze.width @ "x" @ %maze.height @ ")", 2, 2);
	%maze.dump();
	%maze.delete();
	if(isObject($BuildMacroSO))
		$BuildMacroSO.delete();
	$BuildMacroSO = %macro;
}

function MazeSO::dump(%this)
{
	%echo = "";
	%w = %this.width;
	%h = %this.height;
	for(%a=0;%a<(%h+1);%a++)
	{
		%echo = %echo @ "#";
		for(%b=0;%b<%w;%b++)
			%echo = %echo @ (%this.horizontal[%b, %a] ? "##" : " #");
		if(%a != %h)
		{
			%echo = %echo @ "\n";
			for(%c=0;%c<(%w+1);%c++)
				%echo = %echo @ (%this.vertical[%c, %a] ? "#" : " ") @ (%c != %w ? " " : "");
			%echo = %echo @ "\n";
		}
	}
	echo(%echo);
}
// --------------------
// Frontier Control System version 0.1
// Author: firzencode
// --------------------
int gCounter = 0;


void Main()
{
	FCS fcs = new FCS(GridTerminalSystem);
	fcs.Run();
	gCounter = fcs.UpdateCounter(gCounter);
	fcs.CallTimer();
}

public class FCS
{
	string STATE_IDLE = "Idle";
	string STATE_RESET_TO_IDLE = "ResetToIdle";
	string STATE_WALK_LEFT = "WalkLeft";
	string STATE_WALK_RIGHT = "WalkRight";
	
	string CMD_NONE = "cmd_none";
	string CMD_RESET_TO_IDLE = "cmd_reset_to_idle";
	string CMD_WALK_LEFT= "cmd_walk_left";
	string CMD_WALK_RIGHT = "cmd_walk_right";
	string CMD_WALK_TURN_LEFT = "cmd_walk_turn_left";
	string CMD_WALK_TURN_RIGHT = "cmd_walk_turn_right";
	string CMD_WALK_TURN_RESET = "cmd_walk_turn_reset";
	
    private const string mNameRotorLeftUpA = "Rotor Left Up A";
    private const string mNameRotorLeftUpB = "Rotor Left Up B";
    private const string mNameRotorRightUpA = "Rotor Right Up A";
    private const string mNameRotorRightUpB = "Rotor Right Up B";

    private const string mNameRotorLeftDownA = "Rotor Left Down A";
    private const string mNameRotorLeftDownB = "Rotor Left Down B";
    private const string mNameRotorRightDownA = "Rotor Right Down A";
    private const string mNameRotorRightDownB = "Rotor Right Down B";

    private const string mNamePistonLeftA = "Piston Left A";
    private const string mNamePistonLeftB = "Piston Left B";
    private const string mNamePistonLeftC = "Piston Left C";
    private const string mNamePistonLeftD = "Piston Left D";

    private const string mNamePistonRightA = "Piston Right A";
    private const string mNamePistonRightB = "Piston Right B";
    private const string mNamePistonRightC = "Piston Right C";
    private const string mNamePistonRightD = "Piston Right D";

    private const string mNameLandingGearLeftA = "Landing Gear Left A";
    private const string mNameLandingGearLeftB = "Landing Gear Left B";
    private const string mNameLandingGearLeftC = "Landing Gear Left C";
    private const string mNameLandingGearLeftD = "Landing Gear Left D";

    private const string mNameLandingGearRightA = "Landing Gear Right A";
    private const string mNameLandingGearRightB = "Landing Gear Right B";
    private const string mNameLandingGearRightC = "Landing Gear Right C";
    private const string mNameLandingGearRightD = "Landing Gear Right D";

	private const string mNameRotorLeftFootUp = "Rotor Left Foot Up";
	private const string mNameRotorLeftFootDown = "Rotor Left Foot Down";
	
	private const string mNameRotorRightFootUp = "Rotor Right Foot Up";
	private const string mNameRotorRightFootDown = "Rotor Right Foot Down";
	
    private const string mNamePanelCmd = "$Cmd";
    private const string mNamePanelLogout = "$Logout";
    private const string mNameStateMem = "$State";
	private const string mNameTurnMem = "$Turn";
	private const char mNameSpliter = '#';
	
	private const string TURN_LEFT_MARK = "Left";
	private const string TURN_RIGHT_MARK = "Right";
	private const string TURN_NONE_MARK = "None";
	
	private const string mNameTimer = "Timer Block Trigger";
		
	private List<IMyMotorStator> mListRotorLeftUp;
	private List<IMyMotorStator> mListRotorLeftDown;
	private List<IMyMotorStator> mListRotorRightUp;
	private List<IMyMotorStator> mListRotorRightDown;
	
	private List<IMyPistonBase> mListPistonLeft;
	private List<IMyPistonBase> mListPistonRight;
	
	private List<IMyLandingGear> mListGearLeft;
	private List<IMyLandingGear> mListGearRight;
	
	private List<IMyMotorStator> mListRotorLeftFoot;
	private List<IMyMotorStator> mListRotorRightFoot;
	
	private IMyTerminalBlock mBlockCmd;
	private IMyTerminalBlock mBlockLogout;
	private IMyTerminalBlock mBlockStateMem;
	private IMyTerminalBlock mBlockTurnMem;
	
	private IMyTimerBlock mBlockTimer;
	private IMyGridTerminalSystem mGTS;
		
	private float mRotorLimit = 30;
	private float mPistonSpeed = 0.5f;
	private float mRotorSpeed = 6;
	private float mRotorTurnSpeed = 3;
	
    public FCS(IMyGridTerminalSystem gts)
    {
		mGTS = gts;
		InitBlocks();	
    }
	
	public void Run()
	{
		string suffix = GetSuffix(mBlockCmd);
		if (suffix == CMD_RESET_TO_IDLE)
		{
			exeCmdResetToIdle();
			SetSuffix(mBlockCmd, mNamePanelCmd, CMD_NONE);
		}
		else if (suffix == CMD_WALK_LEFT)
		{
			exeCmdWalkLeft();
			SetSuffix(mBlockCmd, mNamePanelCmd, CMD_NONE);
		}
		else if (suffix == CMD_WALK_RIGHT)
		{
			exeCmdWalkRight();
			SetSuffix(mBlockCmd, mNamePanelCmd, CMD_NONE);
		}
		else if (suffix == CMD_WALK_TURN_LEFT)
		{
			exeCmdWalkTurnLeft();
			SetSuffix(mBlockCmd, mNamePanelCmd, CMD_NONE);
		}
		else if (suffix == CMD_WALK_TURN_RIGHT)
		{
			exeCmdWalkTurnRight();
			SetSuffix(mBlockCmd, mNamePanelCmd, CMD_NONE);
		}
		else if (suffix == CMD_WALK_TURN_RESET)
		{
			exeCmdWalkTurnReset();
			SetSuffix(mBlockCmd, mNamePanelCmd, CMD_NONE);
		}

		string state = GetSuffix(mBlockStateMem);
		if (state == STATE_IDLE)
		{
			stateIdle();
			return;
		}
		else if (state == STATE_RESET_TO_IDLE)
		{
			stateResetToIdle();
			return;
		}
		else if (state == STATE_WALK_LEFT)
		{
			stateWalkLeft();
			return;
		}
		else if (state == STATE_WALK_RIGHT)
		{
			stateWalkRight();
			return;
		}
		else
		{
			state = STATE_IDLE;
			SetSuffix(mBlockStateMem, mNameStateMem, STATE_IDLE);
		}
	}
	
	public int UpdateCounter(int counter)
	{
		int res = counter + 1;
		if (res >= 4)
		{
			res = 0;
		}
		
		string strCounter = "";
		
		if (res == 0)
		{
			strCounter = "-";
		}
		
		if (res == 1)
		{
			strCounter = "\\";
		}
		
		if (res == 2)
		{
			strCounter = "|";
		}
		
		if (res == 3)
		{
			strCounter = "/";
		}
		
		Logout(mBlockLogout, strCounter);
		 
		return res;
	}
	
	// call timer to do next trigger
	public void CallTimer()
	{
		DoAction(mBlockTimer, "TriggerNow");
		//DoAction(mBlockTimer, "Start");
	}
	
	private void InitBlocks()
	{
		mListRotorLeftUp = new List<IMyMotorStator>();
		mListRotorLeftDown = new List<IMyMotorStator>();
		mListRotorRightUp = new List<IMyMotorStator>();
		mListRotorRightDown = new List<IMyMotorStator>();
		
		mListRotorLeftUp.Add((IMyMotorStator)GetBlock(mNameRotorLeftUpA));
		mListRotorLeftUp.Add((IMyMotorStator)GetBlock(mNameRotorLeftUpB));
		
		mListRotorLeftDown.Add((IMyMotorStator)GetBlock(mNameRotorLeftDownA));
		mListRotorLeftDown.Add((IMyMotorStator)GetBlock(mNameRotorLeftDownB));
		
		mListRotorRightUp.Add((IMyMotorStator)GetBlock(mNameRotorRightUpA));
		mListRotorRightUp.Add((IMyMotorStator)GetBlock(mNameRotorRightUpB));
		
		mListRotorRightDown.Add((IMyMotorStator)GetBlock(mNameRotorRightDownA));
		mListRotorRightDown.Add((IMyMotorStator)GetBlock(mNameRotorRightDownB));
		
		mListPistonLeft = new List<IMyPistonBase>();
		mListPistonRight = new List<IMyPistonBase>();
		
		mListPistonLeft.Add((IMyPistonBase)GetBlock(mNamePistonLeftA));
		mListPistonLeft.Add((IMyPistonBase)GetBlock(mNamePistonLeftB));
		mListPistonLeft.Add((IMyPistonBase)GetBlock(mNamePistonLeftC));
		mListPistonLeft.Add((IMyPistonBase)GetBlock(mNamePistonLeftD));
		
		mListPistonRight.Add((IMyPistonBase)GetBlock(mNamePistonRightA));
		mListPistonRight.Add((IMyPistonBase)GetBlock(mNamePistonRightB));
		mListPistonRight.Add((IMyPistonBase)GetBlock(mNamePistonRightC));
		mListPistonRight.Add((IMyPistonBase)GetBlock(mNamePistonRightD));
		
		mListGearLeft = new List<IMyLandingGear>();
		mListGearRight = new List<IMyLandingGear>();
		
		mListGearLeft.Add((IMyLandingGear)GetBlock(mNameLandingGearLeftA));
		mListGearLeft.Add((IMyLandingGear)GetBlock(mNameLandingGearLeftB));
		mListGearLeft.Add((IMyLandingGear)GetBlock(mNameLandingGearLeftC));
		mListGearLeft.Add((IMyLandingGear)GetBlock(mNameLandingGearLeftD));
		
		mListGearRight.Add((IMyLandingGear)GetBlock(mNameLandingGearRightA));
		mListGearRight.Add((IMyLandingGear)GetBlock(mNameLandingGearRightB));
		mListGearRight.Add((IMyLandingGear)GetBlock(mNameLandingGearRightC));
		mListGearRight.Add((IMyLandingGear)GetBlock(mNameLandingGearRightD));
		
		mBlockCmd = FirstBlockWithPrefix(mNamePanelCmd);
		mBlockLogout = FirstBlockWithPrefix(mNamePanelLogout);
		mBlockStateMem = FirstBlockWithPrefix(mNameStateMem);
		mBlockTimer = (IMyTimerBlock)GetBlock(mNameTimer);
		mBlockTurnMem = FirstBlockWithPrefix(mNameTurnMem);
		
		mListRotorLeftFoot = new List<IMyMotorStator>();
		mListRotorRightFoot = new List<IMyMotorStator>();
		
		mListRotorLeftFoot.Add((IMyMotorStator)GetBlock(mNameRotorLeftFootUp));
		mListRotorLeftFoot.Add((IMyMotorStator)GetBlock(mNameRotorLeftFootDown));
		
		mListRotorRightFoot.Add((IMyMotorStator)GetBlock(mNameRotorRightFootUp));
		mListRotorRightFoot.Add((IMyMotorStator)GetBlock(mNameRotorRightFootDown));
	}

	// ------------- Cmd ---------------
	private void exeCmdResetToIdle()
	{
		bool mLeftUpAtForward = true;
		bool mLeftDownAtForward = true;
		bool mRightUpAtForward = true;
		bool mRightDownAtForward = true;
		
		// reset pistion
		ExPistonControl(mListPistonLeft, -mPistonSpeed);
		ExPistonControl(mListPistonRight, -mPistonSpeed);
		
		float leftUpDegree = (float)GetRotorDegree(mListRotorLeftUp[0]);
		float leftDownDegree = (float)GetRotorDegree(mListRotorLeftDown[0]);
		float rightUpDegree = (float)GetRotorDegree(mListRotorRightUp[0]);
		float rightDownDegree = (float)GetRotorDegree(mListRotorRightDown[0]);
		
		mLeftUpAtForward = leftUpDegree > 0 ? true : false;
		mLeftDownAtForward = leftDownDegree > 0 ? true : false;
		mRightUpAtForward = rightUpDegree > 0 ? true: false;
		mRightDownAtForward = rightDownDegree > 0 ? true : false;
		
		if (mLeftUpAtForward)
		{
			ExRotorRotate(mListRotorLeftUp, false, mRotorSpeed);
			// ExRotorLimit(mListRotorLeftUp, mRotorLimit, 0);
		}
		else
		{
			// ExRotorLimit(mListRotorLeftUp, 0, -mRotorLimit);
			ExRotorRotate(mListRotorLeftUp, true, mRotorSpeed);
		}
		
		
		if (mLeftDownAtForward)
		{
			ExRotorRotate(mListRotorLeftDown, false, mRotorSpeed);
			// ExRotorLimit(mListRotorLeftDown, mRotorLimit, 0);
		}
		else
		{
			ExRotorRotate(mListRotorLeftDown, true, mRotorSpeed);
			// ExRotorLimit(mListRotorLeftDown, 0, -mRotorLimit);
		}
		
		if (mRightUpAtForward)
		{
			ExRotorRotate(mListRotorRightUp, false, mRotorSpeed);
			// ExRotorLimit(mListRotorRightUp, mRotorLimit, 0);
		}
		else
		{
			ExRotorRotate(mListRotorRightUp, true, mRotorSpeed);
			// ExRotorLimit(mListRotorRightUp, 0, -mRotorLimit);
		}
		
		if (mRightDownAtForward)
		{
			ExRotorRotate(mListRotorRightDown, false, mRotorSpeed);
			// ExRotorLimit(mListRotorRightDown, mRotorLimit, 0);
		}
		else
		{
			ExRotorRotate(mListRotorRightDown, true, mRotorSpeed);
			// ExRotorLimit(mListRotorRightDown, 0, -mRotorLimit);
		}
	
		// ExRotorLimit(mListRotorLeftUp, 1, 0);
		// ExRotorLimit(mListRotorLeftDown, 1, 0);
		// ExRotorLimit(mListRotorRightUp, 1, 0);
		// ExRotorLimit(mListRotorRightDown, 1, 0);
		saveState(STATE_RESET_TO_IDLE);
		
		// reset turn state
		ExSetTurnState(0);
	}
	
	private void exeCmdWalkLeft()
	{
		// left walk forward
		bool moveForward = true;
		
		ExRotorRotate(mListRotorLeftUp, moveForward, mRotorSpeed);
		ExRotorRotate(mListRotorLeftDown, !moveForward, mRotorSpeed);
		ExPistonControl(mListPistonLeft, -mPistonSpeed);
		ExGearUnlock(mListGearLeft);
		
		// right walk backward
		ExRotorRotate(mListRotorRightUp, !moveForward, mRotorSpeed);
		ExRotorRotate(mListRotorRightDown, moveForward, mRotorSpeed);
		ExGearLock(mListGearRight);
		
		saveState(STATE_WALK_LEFT);
	}
	
	private void exeCmdWalkRight()
	{
		// right walk forard
		bool moveForward = true;
		
		ExRotorRotate(mListRotorRightUp, moveForward, mRotorSpeed);
		ExRotorRotate(mListRotorRightDown, !moveForward, mRotorSpeed);
		ExPistonControl(mListPistonRight, -mPistonSpeed);
		ExGearUnlock(mListGearRight);
		
		// left walk backward 
		ExRotorRotate(mListRotorLeftUp, !moveForward, mRotorSpeed);
		ExRotorRotate(mListRotorLeftDown, moveForward, mRotorSpeed);
		ExGearLock(mListGearLeft);
		
		saveState(STATE_WALK_RIGHT);
	}

	private void exeCmdWalkTurnLeft()
	{
		SetSuffix(mBlockTurnMem, mNameTurnMem, TURN_LEFT_MARK);
	}
	
	private void exeCmdWalkTurnRight()
	{
		SetSuffix(mBlockTurnMem, mNameTurnMem, TURN_RIGHT_MARK);
	}
	
	private void exeCmdWalkTurnReset()
	{
		SetSuffix(mBlockTurnMem, mNameTurnMem, TURN_NONE_MARK);
	}
	
	// ------------- States ------------
	
	private void saveState(string state)
	{
		SetSuffix(mBlockStateMem, mNameStateMem, state);
	}
	
	private void stateIdle()
	{
	
	}
	
	private void stateResetToIdle()
	{		
		// left Up check
		float degreeLU = (float)GetRotorDegree(mListRotorLeftUp[0]);
		
		if (mListRotorLeftUp[0].Velocity > 0)
		{
			if(degreeLU >= 0)
			{
				ExRotorLimit(mListRotorLeftUp, mRotorLimit, -mRotorLimit);
				ExRotorStop(mListRotorLeftUp);
			}
		}
		else if (mListRotorLeftUp[0].Velocity < 0)
		{
			if(degreeLU <= 0)
			{
				ExRotorLimit(mListRotorLeftUp, mRotorLimit, -mRotorLimit);
				ExRotorStop(mListRotorLeftUp);
			}
		}
		
		
		float degreeLD = (float)GetRotorDegree(mListRotorLeftDown[0]);
		// Left Down check
		if (mListRotorLeftDown[0].Velocity > 0)
		{
			if(degreeLD >= 0)
			{
				ExRotorLimit(mListRotorLeftDown, mRotorLimit, -mRotorLimit);
				ExRotorStop(mListRotorLeftDown);
			}
		}
		else if (mListRotorLeftDown[0].Velocity < 0)
		{
			if(degreeLD <= 0)
			{
				
				ExRotorLimit(mListRotorLeftDown, mRotorLimit, -mRotorLimit);
				ExRotorStop(mListRotorLeftDown);
			}
		}
		
		float degreeRU = (float)GetRotorDegree(mListRotorRightUp[0]);
		// Right Up check
		if (mListRotorRightUp[0].Velocity > 0)
		{
			if(degreeRU >= 0)
			{
				ExRotorLimit(mListRotorRightUp, mRotorLimit, -mRotorLimit);
				ExRotorStop(mListRotorRightUp);
			}
		}
		else if (mListRotorRightUp[0].Velocity < 0)
		{
			if(degreeRU <= 0)
			{
				
				ExRotorLimit(mListRotorRightUp, mRotorLimit, -mRotorLimit);
				ExRotorStop(mListRotorRightUp);
			}
		}
		
		float degreeRD = (float)GetRotorDegree(mListRotorRightDown[0]);
		// Right Down check
		if (mListRotorRightDown[0].Velocity > 0)
		{
			if(degreeRD >= 0)
			{
				
				ExRotorLimit(mListRotorRightDown, mRotorLimit, -mRotorLimit);
				ExRotorStop(mListRotorRightDown);
			}
		}
		else if (mListRotorRightDown[0].Velocity < 0)
		{
			if(degreeRD <= 0)
			{
				
				ExRotorLimit(mListRotorRightDown, mRotorLimit, -mRotorLimit);
				ExRotorStop(mListRotorRightDown);
			}
		}
		
		// --- FOOT ---
		
		ExTurnCheckToReset(mListRotorLeftFoot);
		ExTurnCheckToReset(mListRotorRightFoot);
		
		// float? degreeFootLeft = GetRotorDegree(mListRotorLeftFoot[0]);
		// if (degreeFootLeft < 0)
		// {
			// ExRotorRotate(mListRotorLeftFoot, true, mRotorSpeed);
		// }
		// else if (degreeFootLeft > 0)
		// {
			// ExRotorRotate(mListRotorLeftFoot, false, mRotorSpeed);
		// }else
		// {
			// ExRotorStop(mListRotorLeftFoot);
		// }
		
		// float? degreeFootRight = GetRotorDegree(mListRotorRightFoot[0]);
		// if (degreeFootRight < 0)
		// {
			// ExRotorRotate(mListRotorRightFoot, true, mRotorSpeed);
		// }
		// else if (degreeFootRight > 0)
		// {
			// ExRotorRotate(mListRotorRightFoot, false, mRotorSpeed);
		// }else
		// {
			// ExRotorStop(mListRotorRightFoot);
		// }
		
		// ------------
		
		if (mListRotorLeftUp[0].Velocity == 0 && mListRotorLeftDown[0].Velocity == 0
		&& mListRotorRightUp[0].Velocity == 0 && mListRotorRightDown[0].Velocity == 0
		&& mListRotorLeftFoot[0].Velocity == 0 && mListRotorRightFoot[0].Velocity == 0)
		{
			ExPistonControl(mListPistonLeft, mPistonSpeed);
			ExPistonControl(mListPistonRight, mPistonSpeed);
			saveState(STATE_IDLE);
		}
	}
	
	private void stateWalkLeft()
	{
		float? degreeLeft = GetRotorDegree(mListRotorLeftUp[0]);
		if (degreeLeft >= 15)
		{
			ExPistonControl(mListPistonLeft, mPistonSpeed);
		}
		
		float? degreeRight = GetRotorDegree(mListRotorRightUp[0]);
		
		// rotor is ready
		if (degreeLeft >= mRotorLimit && degreeRight <= -mRotorLimit)
		{
			ExGearLock(mListGearLeft);
			if (ExIsGearLockPart(mListGearLeft, 1))
			{
				SendCmdDelay(CMD_WALK_RIGHT);
			}
		}
		
		// turn way
		int turnWay = ExGetTurnState();
		if (turnWay == 1)
		{
			// turn left
			ExTurnCheckToLeft(mListRotorLeftFoot);
			ExTurnCheckToReset(mListRotorRightFoot);
			
		}
		else if (turnWay == -1)
		{
			// turn right
			ExTurnCheckToReset(mListRotorLeftFoot);
			ExTurnCheckToReset(mListRotorRightFoot);
		}
		else
		{
			// turn none
			ExTurnCheckToReset(mListRotorLeftFoot);
			ExTurnCheckToReset(mListRotorRightFoot);
		}
	}
	
	private void stateWalkRight()
	{
		float? degreeRight = GetRotorDegree(mListRotorRightUp[0]);
		if (degreeRight >= 15)
		{
			ExPistonControl(mListPistonRight, mPistonSpeed);
		}
		
		float? degreeLeft = GetRotorDegree(mListRotorLeftUp[0]);
		
		// rotor is ready
		if (degreeRight >= mRotorLimit && degreeLeft <= -mRotorLimit)
		{
			ExGearLock(mListGearRight);
			if (ExIsGearLockPart(mListGearRight, 1))
			{
				SendCmdDelay(CMD_WALK_LEFT);
			}
		}
		
		int turnWay = ExGetTurnState();
		if (turnWay == 1)
		{
			// turn left
			ExTurnCheckToReset(mListRotorLeftFoot);
			ExTurnCheckToReset(mListRotorRightFoot);
			
		}
		else if (turnWay == -1)
		{
			// turn right
			ExTurnCheckToReset(mListRotorLeftFoot);
			ExTurnCheckToRight(mListRotorRightFoot);
		}
		else
		{
			// turn none
			ExTurnCheckToReset(mListRotorLeftFoot);
			ExTurnCheckToReset(mListRotorRightFoot);
		}
	}
	
	
	
	// ------------- Tools -------------
	
	// --- Landing Gear ---
	
	public bool IsGearLocked(IMyLandingGear gear)
	{
		string info = gear.DetailedInfo;
		return info.Contains("Locked");
	}

	public bool IsGearReadyToLock(IMyLandingGear gear)
	{
		return gear.DetailedInfo.Contains("Ready To Lock");
	}

	public bool IsGearUnlocked(IMyLandingGear gear)
	{
		return gear.DetailedInfo.Contains("Unlocked");
	}
	
	// --- Rotor ---
	
	public float? GetRotorDegree(IMyTerminalBlock block)
	{
		return TryExtractFloat(block.DetailedInfo, @"(-?\d+)°");
	}
	
	public bool IsMotorReachedUpperLimit(IMyMotorStator rotor)
	{
		return (GetRotorDegree((IMyTerminalBlock)rotor) >= rotor.UpperLimit * 180 / Math.PI);
	}
	
	public bool IsMotorReachedLowerLimit(IMyMotorStator rotor)
	{
		return (GetRotorDegree((IMyTerminalBlock)rotor) <= rotor.LowerLimit * 180 / Math.PI);
	}
	
	// --- Piston ---
	// public bool IsPistonExpanded(IMyPistonBase piston)
	// {
		// var piston = AsPiston(block);
		// have to use '.ToString()' because the rounded value don't exactly match to the ui value
		// return (Math.Round(piston.MaxLimit, 1).ToString() == GetPistonPosition(block).ToString());
	// }
	
	// public bool IsPistonContracted(IMyPistonBase piston)
	// {
		// var piston = AsPiston(block);
		// have to use '.ToString()' because the rounded value don't exactly match to the ui value
		// return (Math.Round(piston.MinLimit, 1).ToString() == GetPistonPosition(block).ToString());
	// }
	
	
	// --- Utils ---
	
	public IMyTerminalBlock GetBlock(string name)
    {
        IMyTerminalBlock block = mGTS.GetBlockWithName(name);
		if (block == null)
		{
			throw new Exception("Can't find block: " + name);
		}
		return block;
	}

	public IMyTerminalBlock FirstBlockWithPrefix(string prefix) {    
		List<IMyTerminalBlock> allBlocks = mGTS.Blocks;    
		for (int i = 0; i < allBlocks.Count; ++i) {    
			if (allBlocks[i].CustomName.StartsWith(prefix))    
				return allBlocks[i];    
		}    
		throw new Exception(String.Format("FirstBlockWithPrefix: prefix \"{0}\" not found", prefix));    
	}  
	
	public void DoAction(IMyTerminalBlock block, String actionName)
	{
		block.GetActionWithName(actionName).Apply(block);
	}
	
	public void SetValueFloat(IMyTerminalBlock block, string valueName, float value)
	{
		block.SetValueFloat(valueName, value);
	}
	
	public float? TryExtractFloat(string value, string pattern)
	{
		var matches = System.Text.RegularExpressions.Regex.Match(value, pattern);

		if (!matches.Groups[1].Success)
			return null;

		return float.Parse(matches.Groups[1].Value);
	}
	
	public void Logout(IMyTerminalBlock block, string log)
	{
		block.SetCustomName(mNamePanelLogout + mNameSpliter + log);
	}
	
	public string GetSuffix(IMyTerminalBlock block)
	{
		string name = block.CustomName;
		string[] l = name.Split(mNameSpliter);
		if (l.Length >= 2)
		{
			return l[1];
		}
		return "";
	}
	
	public void SetSuffix(IMyTerminalBlock block, string perfix, string suffix)
	{
		block.SetCustomName(perfix + mNameSpliter + suffix);
	}
	
	public void SendCmdDelay(string command)
	{
		SetSuffix(mBlockCmd, mNamePanelCmd, command);
	}
	
	// ---------- Utils Ex ----------
	
	public void ExRotorRotate(List<IMyMotorStator> list, bool isForward, float speed)
	{
		float flag = 1;
		if (!isForward)
		{
			flag *= -1;
		}
		
		SetValueFloat(list[0], "Velocity", speed * flag);
		SetValueFloat(list[1], "Velocity", -speed * flag);
		// SetValueFloat(list[1], "Velocity", 0);
	}
	
	public void ExRotorStop(List<IMyMotorStator> list)
	{
		SetValueFloat(list[0], "Velocity", 0);
		SetValueFloat(list[1], "Velocity", 0);
	}
	
	public void ExRotorLimit(List<IMyMotorStator> list, float upperLimit, float lowerLimit)
	{
		SetValueFloat(list[0], "UpperLimit", upperLimit);
		SetValueFloat(list[0], "LowerLimit", lowerLimit);
		SetValueFloat(list[1], "UpperLimit", -lowerLimit);
		SetValueFloat(list[1], "LowerLimit", -upperLimit);
		
	}
	
	public void ExPistonControl(List<IMyPistonBase> list, float speed)
	{
		for(int i = 0; i < list.Count; i++)
		{
			SetValueFloat(list[i], "Velocity", speed);
		}
	}
	
	public void ExGearLock(List<IMyLandingGear> list)
	{
		for(int i = 0; i < list.Count; i++)
		{
			DoAction(list[i],"Lock");
		}
	}
	
	public void ExGearUnlock(List<IMyLandingGear> list)
	{
		for(int i = 0; i < list.Count; i++)
		{
			DoAction(list[i],"Unlock");
		}
	}
	
	public bool ExIsGearLock(List<IMyLandingGear> list)
	{
		return (IsGearLocked(list[0]) && IsGearLocked(list[1]) && IsGearLocked(list[2]) && IsGearLocked(list[3]));
	}
	
	public bool ExIsGearUnlock(List<IMyLandingGear> list)
	{
		return (IsGearUnlocked(list[0]) && IsGearUnlocked(list[1]) && IsGearUnlocked(list[2]) && IsGearUnlocked(list[3]));
	}
	
	public bool ExIsGearLockPart(List<IMyLandingGear> list, int lockCount)
	{
		int sum = 0;
		for(int i = 0; i < list.Count; i++){
			if(IsGearLocked(list[i]))
			{
				sum ++;
			}
		}
		
		if (sum >= lockCount)
		{
			return true;
		}else
		{	
			return false;
		}
	}

	private int ExGetTurnState()
	{
		string str = GetSuffix(mBlockTurnMem);
		if (str == TURN_LEFT_MARK)
		{
			return 1;
		}
		else if (str == TURN_RIGHT_MARK)
		{
			return -1;
		}
		else
		{
			return 0;
		}
	}
	
	private void ExSetTurnState(int way)
	{
		if (way == 1)
		{
			SetSuffix(mBlockTurnMem, mNameTurnMem, TURN_LEFT_MARK);
		}
		else if (way == -1)
		{
			SetSuffix(mBlockTurnMem, mNameTurnMem, TURN_RIGHT_MARK);
		}
		else
		{
			SetSuffix(mBlockTurnMem, mNameTurnMem, TURN_NONE_MARK);
		}
	}	
	
	private void ExTurnCheckToReset(List<IMyMotorStator> list)
	{
		float? degree = GetRotorDegree(list[0]);
		if (degree < 0)
		{
			ExRotorRotate(list, true, mRotorTurnSpeed);
		}
		else if (degree > 0)
		{
			ExRotorRotate(list, false, mRotorTurnSpeed);
		}else
		{
			ExRotorStop(list);
		}
	}
	
	private void ExTurnCheckToLeft(List<IMyMotorStator> list)
	{
		ExRotorRotate(list, true, mRotorTurnSpeed);
	}
	
	private void ExTurnCheckToRight(List<IMyMotorStator> list)
	{
		ExRotorRotate(list, false, mRotorTurnSpeed);
	}
}
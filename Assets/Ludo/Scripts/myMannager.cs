using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class myMannager 
{
	private static myMannager instance;

	private myMannager()
	{
	}

	public static myMannager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new myMannager();
			}
			return instance;
		}
	}

	public List<Sprite> flags = new List<Sprite>();
	public List<string> names=new List<string>();
	public int myplayerIndex;
	public bool isMultiPlayer;




}

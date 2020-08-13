using ProtoBuf;
using protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfoModel
{
    private static UserInfoModel instance;
    public static UserInfoModel Instance
    {
        set { instance = value; }
        get
        {
            if (instance == null)
                instance = new UserInfoModel();
            return instance;
        }
    }

    public string currentToken;//当前登录的token
    public string userId = "";//用户id 
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Excel;
using System.Data;
using System;

public class ReadExcel : Editor
{
    [MenuItem("ReadConfigExcel/Read")]
    public static void ReadConfig()
    {
        ConfigAsset _config = Resources.Load<ConfigAsset>("Config");
        _config.config_CardRandomRewards.Clear();
        _config.config_CardExtraRewards.Clear();
        _config.config_WheelRewards.Clear();
        _config.config_Tasks.Clear();
        _config.config_Gifts.Clear();
        _config.config_LuckySpins.Clear();
        _config.config_MoreBalls.Clear();
        _config.config_GiftBallons.Clear();
        _config.config_DrawBoxes.Clear();
        _config.config_Pigs.Clear();

        string XlsxPath = Application.dataPath + "/Config.xlsx";
        if (!File.Exists(XlsxPath))
        {
            Debug.LogError("Read MG_ConfigXlsx File Error : file is not exist.");
            return;
        }
        FileStream fs = new FileStream(XlsxPath, FileMode.Open, FileAccess.Read);
        IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet dataSet = reader.AsDataSet();
        reader.Dispose();
        if (dataSet is null)
        {
            Debug.LogError("Read MG_ConfigXlsx File Error : file is empty.");
            return;
        }

        #region 卡片奖励随机
        DataTable cardRewardTable = dataSet.Tables[0];
        int rowCount0 = cardRewardTable.Rows.Count;
        for(int i = 1; i < rowCount0; i++)
        {
            var tempRow = cardRewardTable.Rows[i];
            if (string.IsNullOrEmpty(tempRow[0].ToString()))
                continue;
            Config_CardRandomReward cardRandomReward = new Config_CardRandomReward()
            {
                cash_max = int.Parse(tempRow[1].ToString()),
                cash_cardNum = int.Parse(tempRow[2].ToString()),
                cash_rewardNum = int.Parse(tempRow[3].ToString()),
                gold_max = int.Parse(tempRow[5].ToString()),
                gold_cardNum = int.Parse(tempRow[6].ToString()),
                gold_rewardNum = int.Parse(tempRow[7].ToString())
            };
            _config.config_CardRandomRewards.Add(cardRandomReward);
        }
        #endregion
        #region 卡片额外奖励随机
        DataTable extraRewardTable = dataSet.Tables[1];
        int rowCount1 = extraRewardTable.Rows.Count;
        for(int i = 1; i < rowCount1; i++)
        {
            var tempRow = extraRewardTable.Rows[i];
            if (string.IsNullOrEmpty(tempRow[0].ToString()))
                continue;
            Config_CardExtraReward cardExtraReward = new Config_CardExtraReward()
            {
                rewardType = (ExtraReward)Enum.Parse(typeof(ExtraReward), tempRow[0].ToString()),
                weight = int.Parse(tempRow[1].ToString()),
                numRange = new Range(int.Parse(tempRow[2].ToString()), int.Parse(tempRow[3].ToString())),
                mustBeInThreeTimes = int.Parse(tempRow[4].ToString()),
                appearLimitType = (Reward)Enum.Parse(typeof(Reward), tempRow[5].ToString()),
                appearLimitMax = int.Parse(tempRow[6].ToString())
            };
            _config.config_CardExtraRewards.Add(cardExtraReward);
        }
        #endregion
        #region 转盘随机奖励
        DataTable wheelRewardTable = dataSet.Tables[2];
        int rowCount2 = wheelRewardTable.Rows.Count;
        for(int i = 1; i < rowCount2; i++)
        {
            var tempRow = wheelRewardTable.Rows[i];
            if (string.IsNullOrEmpty(tempRow[0].ToString()))
                continue;
            Config_WheelReward wheelReward = new Config_WheelReward()
            {
                rewardType = (Reward)Enum.Parse(typeof(Reward), tempRow[0].ToString()),
                rewardNum = int.Parse(tempRow[2].ToString()),
                weight = int.Parse(tempRow[3].ToString()),
                appearLimitType = (Reward)Enum.Parse(typeof(Reward), tempRow[5].ToString()),
                appearLimitMax = int.Parse(tempRow[6].ToString())
            };
            wheelReward.blackBox = new List<int>();
            string[] blackBoxStringList = tempRow[4].ToString().Split(',');
            int blackBoxListLength = blackBoxStringList.Length;
            for (int index = 0; index < blackBoxListLength; index++)
                if (!string.IsNullOrEmpty(blackBoxStringList[index]))
                    wheelReward.blackBox.Add(int.Parse(blackBoxStringList[index]));
            wheelReward.randomMultipleAndWeight = new List<Vector2Int>();
            string[] multiplesStringList = tempRow[7].ToString().Split(',');
            int multipleListLength = multiplesStringList.Length;
            for(int index = 0; index < multipleListLength; index++)
            {
                if (string.IsNullOrEmpty(multiplesStringList[index]))
                    continue;
                string[] singleMultipleAndWeight = multiplesStringList[index].Split('/');
                if (singleMultipleAndWeight.Length != 2)
                    throw new ArgumentOutOfRangeException("转盘倍数设置错误");
                wheelReward.randomMultipleAndWeight.Add(new Vector2Int(int.Parse(singleMultipleAndWeight[0]), int.Parse(singleMultipleAndWeight[1])));
            }
            _config.config_WheelRewards.Add(wheelReward);
        }
        #endregion
        #region 任务
        DataTable taskTable = dataSet.Tables[3];
        int rowCount3 = taskTable.Rows.Count;
        for(int i = 1; i < rowCount3; i++)
        {
            var tempRow = taskTable.Rows[i];
            if (string.IsNullOrEmpty(tempRow[0].ToString()))
                continue;
            Config_Task task = new Config_Task()
            {
                name = tempRow[0].ToString(),
                id = int.Parse(tempRow[1].ToString()),
                taskType = (TaskType)Enum.Parse(typeof(TaskType),tempRow[2].ToString()),
                needTimes = int.Parse(tempRow[3].ToString())
            };
            task.rewardTypes = new List<Reward>();
            string[] rewardTypeStringList = tempRow[4].ToString().Split(',');
            int rewardTypeCount = rewardTypeStringList.Length;
            for (int index = 0; index < rewardTypeCount; index++)
                task.rewardTypes.Add((Reward)Enum.Parse(typeof(Reward), rewardTypeStringList[index]));

            task.rewardNums = new List<int>();
            string[] rewardNumStringList = tempRow[5].ToString().Split(',');
            int rewardNumCount = rewardNumStringList.Length;
            for (int index = 0; index < rewardNumCount; index++)
                task.rewardNums.Add(int.Parse(rewardNumStringList[index]));

            task.appearLimitTypes = new List<Reward>();
            string[] appearLimitTypeStringList = tempRow[6].ToString().Split(',');
            int appearLimitTypeCount = appearLimitTypeStringList.Length;
            for (int index = 0; index < appearLimitTypeCount; index++)
                task.appearLimitTypes.Add((Reward)Enum.Parse(typeof(Reward), appearLimitTypeStringList[index]));

            task.appearLimitMaxes = new List<int>();
            string[] appearLimitMaxStringList = tempRow[7].ToString().Split(',');
            int appearLimitMaxCount = appearLimitMaxStringList.Length;
            for (int index = 0; index < appearLimitMaxCount; index++)
                task.appearLimitMaxes.Add(int.Parse(appearLimitMaxStringList[index]));

            task.randomMultipleRewardTypes = new List<Reward>();
            string[] randomTypeStringList = tempRow[8].ToString().Split(',');
            int typeCount = randomTypeStringList.Length;
            for (int index = 0; index < typeCount; index++)
                task.randomMultipleRewardTypes.Add((Reward)Enum.Parse(typeof(Reward), randomTypeStringList[index]));

            task.randomMultipleAndWeights = new List<Config_MultipleWeights>();
            string[] randomWeightStringList = tempRow[9].ToString().Split(';');
            int weightListCount = randomWeightStringList.Length;
            for(int index = 0; index < weightListCount; index++)
            {
                Config_MultipleWeights multipleWeightList = new Config_MultipleWeights
                {
                    multipleWeights = new List<Vector2Int>()
                };
                string[] multipleWeightStringList = randomWeightStringList[index].Split(',');
                int multipleCount = multipleWeightStringList.Length;
                for(int multipleIndex = 0; multipleIndex < multipleCount; multipleIndex++)
                {
                    string[] multipleWeightString = multipleWeightStringList[multipleIndex].Split('/');
                    multipleWeightList.multipleWeights.Add(new Vector2Int(int.Parse(multipleWeightString[0]), int.Parse(multipleWeightString[1])));
                }
                task.randomMultipleAndWeights.Add(multipleWeightList);
            }
            _config.config_Tasks.Add(task);
        }
        #endregion
        #region 礼品兑换所需数量
        DataTable giftTable = dataSet.Tables[4];
        int rowCount4 = giftTable.Rows.Count;
        for(int i = 1; i < rowCount4; i++)
        {
            var tempRow = giftTable.Rows[i];
            if (string.IsNullOrEmpty(tempRow[0].ToString()))
                continue;
            Config_Gift gift = new Config_Gift()
            {
                giftType = (Reward)Enum.Parse(typeof(Reward), tempRow[0].ToString()),
                redeemNeedNum = int.Parse(tempRow[2].ToString())
            };
            _config.config_Gifts.Add(gift);
        }
        #endregion
        #region 抽奖
        DataTable drawTable = dataSet.Tables[5];
        int rowCount5 = drawTable.Rows.Count;
        for(int i = 1; i < rowCount5; i++)
        {
            var tempRow = drawTable.Rows[i];
            if (string.IsNullOrEmpty(tempRow[0].ToString()))
                continue;
            Config_LuckySpin luckySpin = new Config_LuckySpin()
            {
                rewardType = (Reward)Enum.Parse(typeof(Reward), tempRow[0].ToString()),
                weight = int.Parse(tempRow[1].ToString()),
                rewardNum = int.Parse(tempRow[2].ToString()),
                appearLimitType = (Reward)Enum.Parse(typeof(Reward), tempRow[4].ToString()),
                appearLimitMax = int.Parse(tempRow[5].ToString()),
                firstGetRewardNum = int.Parse(tempRow[6].ToString())
            };
            luckySpin.blackBox = new List<int>();
            string[] blackBoxStringList = tempRow[3].ToString().Split(',');
            int blackBoxSize = blackBoxStringList.Length;
            for (int blackIndex = 0; blackIndex < blackBoxSize; blackIndex++)
                luckySpin.blackBox.Add(int.Parse(blackBoxStringList[blackIndex]));
            _config.config_LuckySpins.Add(luckySpin);
        }
        #endregion
        #region 额外的球
        DataTable ballTable = dataSet.Tables[6];
        int rowCount6 = ballTable.Rows.Count;
        for(int i = 1; i < rowCount6; i++)
        {
            var tempRow = ballTable.Rows[i];
            if (string.IsNullOrEmpty(tempRow[0].ToString()))
                continue;
            Config_MoreBall moreBall = new Config_MoreBall()
            {
                ballCount = int.Parse(tempRow[0].ToString()),
                weight = int.Parse(tempRow[1].ToString()),
                usefulCount = int.Parse(tempRow[2].ToString()),
                unUsefulCount = int.Parse(tempRow[3].ToString()),
                blackBox = int.Parse(tempRow[4].ToString())
            };
            _config.config_MoreBalls.Add(moreBall);
        }
        #endregion
        #region 礼盒气球
        DataTable ballonTable = dataSet.Tables[7];
        int rowCount7 = ballonTable.Rows.Count;
        for(int i = 1; i < rowCount7; i++)
        {
            var tempRow = ballonTable.Rows[i];
            if (string.IsNullOrEmpty(tempRow[0].ToString()))
                continue;
            Config_GiftBallon giftBallon = new Config_GiftBallon()
            {
                rewardType = (Reward)Enum.Parse(typeof(Reward), tempRow[0].ToString()),
                weight = int.Parse(tempRow[1].ToString()),
                rewardNum = int.Parse(tempRow[2].ToString()),
                appearLimitType = (Reward)Enum.Parse(typeof(Reward), tempRow[3].ToString()),
                appearLimitMax = int.Parse(tempRow[4].ToString()),
            };
            _config.config_GiftBallons.Add(giftBallon);
        }
        #endregion
        #region 开宝箱
        DataTable boxTable = dataSet.Tables[8];
        int rowCount8 = boxTable.Rows.Count;
        for(int i = 1; i < rowCount8; i++)
        {
            var tempRow = boxTable.Rows[i];
            if (string.IsNullOrEmpty(tempRow[0].ToString()))
                continue;
            Config_DrawBox drawBox = new Config_DrawBox()
            {
                rewardType = (Reward)Enum.Parse(typeof(Reward), tempRow[0].ToString()),
                weight = int.Parse(tempRow[1].ToString()),
                rewardNum = int.Parse(tempRow[2].ToString()),
                appearLimitType = (Reward)Enum.Parse(typeof(Reward), tempRow[4].ToString()),
                appearLimitMax = int.Parse(tempRow[5].ToString())
            };
            drawBox.blackBox = new List<int>();
            string[] blackBoxStringList = tempRow[3].ToString().Split(',');
            int blackBoxCount = blackBoxStringList.Length;
            for (int blackIndex = 0; blackIndex < blackBoxCount; blackIndex++)
                drawBox.blackBox.Add(int.Parse(blackBoxStringList[blackIndex]));
            _config.config_DrawBoxes.Add(drawBox);
        }
        #endregion
        #region 存钱罐奖励
        DataTable pigTable = dataSet.Tables[9];
        int rowCount9 = pigTable.Rows.Count;
        for(int i = 1; i < rowCount9; i++)
        {
            var tempRow = pigTable.Rows[i];
            if (string.IsNullOrEmpty(tempRow[0].ToString()))
                continue;
            Config_Pig pig = new Config_Pig()
            {
                maxTotalPigCash = int.Parse(tempRow[1].ToString()),
                rewardPigCashNum = int.Parse(tempRow[2].ToString())
            };
            _config.config_Pigs.Add(pig);
        }
        #endregion
        Debug.Log("COMPLETE!");
        EditorUtility.SetDirty(_config);
        AssetDatabase.SaveAssets();
    }
}

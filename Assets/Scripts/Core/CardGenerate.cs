using System.Collections;
using System.Collections.Generic;
using AdsITSoft;
using UnityEngine;
using UnityEngine.UI;

public class CardGenerate : MonoBehaviour,IUIMessage
{
    public static GameState gameState = GameState.Unknow;
    public Animator animator;
    public RectTransform single_panel;
    public Card_Next single_next;
    public Card_Item single_card;
    private const string BINGO = "BINGO";
    private readonly string[] BINGO_COLOR = new string[5]
    {
        "Ball_Red",
        "Ball_Yellow",
        "Ball_Pink",
        "Ball_Green",
        "Ball_Blue"
    };
    private const int Columns_Count = 5;
    static readonly float[] UpOffset = new float[3] { 93, 85, 61.7f };
    static readonly float[] LeftOffset = new float[3] { 21, 19, 14.353f };
    static readonly float[] VerticalInterval = new float[3] { 133, 122, 87.4f };
    static readonly float[] HorizontalInterval = new float[3] { 142, 130.5f, 94 };
    static readonly Vector2[] CardSize = new Vector2[3]
    {
        new Vector2(139.5882f, 129.5011f),
        new Vector2(127.4598f, 120.0679f),
        new Vector2(91.09601f, 85.81294f)
    };
    static readonly Vector2[] PanelSize = new Vector2[3]
    {
        new Vector2(749, 792),
        new Vector2(687.8f, 724),
        new Vector2(495.5067f, 520.6079f)
    };
    static readonly Vector2[][] PanelPos = new Vector2[3][]
    {
        new Vector2[1]{ new Vector2(-5f, -33.6f)},
        new Vector2[2]{ new Vector2(10.59f, 199), new Vector2(9.54f, -549) },
        new Vector2[4]{ new Vector2(-256.1f, 212f), new Vector2(256.1f, 212f), new Vector2(256.1f, -346.8f), new Vector2(-256.1f, -346.8f) }
    };
    static readonly Vector2[] BingoSize = new Vector2[3]
    {
        new Vector2(707.588f,669.8689f),
        new Vector2(649.4596f,612.4473f),
        new Vector2(469.6036f,440.4211f)
    };
    static readonly Vector2[] BingoPos = new Vector2[3]
    {
        new Vector2(0,-32.2f),
        new Vector2(0,-28.8f),
        new Vector2(0,-20.64999f),
    };
    static readonly int[] CardNumTextSize = new int[3]
    {
        85,
        70,
        50
    };
    const int MaxBingoGoldReward = 3;
    const int MaxBingoCashReward = 3;
    [Header("每一列的随机数量")]
    const int NUM_PER_COLUMNS = 15;
    [Header("每张卡片中有多少球")]
    public Range cardBingoNum = new Range(16, 18);
    [Header("球的最大数量")]
    const int MaxBallCount = 27;
    [Header("生成球的间隔时间(秒)")]
    public float generateNextInterval = 5;
    [Header("最多同时显示球的数量")]
    public int maxShowNextCount = 5;
    [Header("bingo板数")]
    public int bingo_panel_num = 1;
    public Text ball_left_numText;
    public Button extra_rewardButton;

    public static CardGenerate Instance;

    private readonly List<RectTransform> all_panel = new List<RectTransform>();
    private readonly List<RectTransform> all_panel_bingo = new List<RectTransform>();
    private readonly List<Card_Item[,]> all_panel_cardItems = new List<Card_Item[,]>();
    private readonly List<Card_ItemData[,]> all_panel_cardItem_Datas = new List<Card_ItemData[,]>();
    private List<List<List<int>>> notBingoCardPos;//只有通过星星道具才会减少
    private List<List<List<int>>> hasRewardCardPos;//只有通过点击卡片bingo才会减少，只有通过金币或现金道具才会增加
    private List<List<List<int>>> noRewardCardPos;//只有通过金币或现金道具或点击才会减少
    private readonly List<int> all_panel_bingoCount = new List<int>();
    private Coroutine generateCor = null;
    private void Awake()
    {
        Instance = this;
        Vector2 defaultPivot = new Vector2(0.5f, 0.5f);
        single_card.Rect.pivot = defaultPivot;
        single_panel.pivot = defaultPivot;
        #region 添加初始个数
        all_panel.Add(single_panel);
        all_panel_bingo.Add(single_panel.GetChild(0).GetComponent<RectTransform>());
        all_panel_cardItems.Add(new Card_Item[Columns_Count, Columns_Count]);
        all_panel_cardItems[0][0, 0] = single_card;
        extra_reward_flyImages.Add(single_extra_reward_flyImage);
        #endregion
        card_Nexts.Add(single_next);
        extra_rewardButton.AddClickEvent(OnExtraButtonClick);
        helpButton.AddClickEvent(OnHelpButtonClick);
        keyButton.AddClickEvent(OnKeyButtonClick);
        UIManager.SetBallTextTransAndKeyTrans(ball_left_numText.transform,keysImage[0].transform, keysImage[1].transform, keysImage[2].transform);
    }
    private void OnExtraButtonClick()
    {
        if (isGuideStep2 && UIManager.PanelShow(PopPanel.GuidePanel))
        {
            //UIManager.SendMessageToPanel((int)BasePanel.GamePanel, (int)PopPanel.GuidePanel);
            //isPause = false;
        }
    }
    bool isGuideStep1 = false;
    bool isGuideStep2 = false;
    private Coroutine GenerateBallCoroutine = null;
    private IEnumerator GenerateCards(int panelNum)
    {
        isGuideStep1 = SaveManager.SaveData.guideStep == (int)GuideType.GameGuide_ClickCard;
        isGuideStep2 = SaveManager.SaveData.guideStep == (int)GuideType.GameGuide_ExtraProp;
        if(isGuideStep1)
            extra_reward_progress_fillImage.transform.parent.gameObject.SetActive(false);
        #region 重置球和额外奖励
        ResetExtraReward();
        if (GenerateBallCoroutine != null)
            StopCoroutine(GenerateBallCoroutine);
        StopCoroutine("AutoIncreaseExtraRewardProgress");
        foreach (var ball in card_Nexts)
            ball.gameObject.SetActive(false);
        ball_left_numText.text = string.Empty;
        #endregion
        #region 确定面板的数字排列间距和大小
        float upOffset;
        float leftOffset;
        float verticalInterval;
        float horizontalInterval;
        Vector2 panelSize;
        Vector2 cardSize;
        Vector2[] panelPos;
        int cardNumTextSize;
        int gameType;
        if (panelNum == 1)
            gameType = 0;
        else if(panelNum==2)
            gameType = 1;
        else
            gameType = 2;
        panelSize = PanelSize[gameType];
        panelPos = PanelPos[gameType];
        upOffset = UpOffset[gameType];
        leftOffset = LeftOffset[gameType];
        verticalInterval = VerticalInterval[gameType];
        horizontalInterval = HorizontalInterval[gameType];
        cardSize = CardSize[gameType];
        cardNumTextSize = CardNumTextSize[gameType];
        for (int panelIndex = 0; panelIndex < panelNum; panelIndex++)
        {
            if (panelIndex > all_panel.Count - 1)
            {
                yield return null;
                RectTransform newPanel = Instantiate(single_panel.gameObject, single_panel.parent).GetComponent<RectTransform>();
                newPanel.SetSiblingIndex(all_panel[panelIndex - 1].GetSiblingIndex() + 1);
                all_panel.Add(newPanel);
                all_panel_bingo.Add(newPanel.Find("Bingo").GetComponent<RectTransform>());
            }
            all_panel[panelIndex].gameObject.SetActive(true);
            Transform bingoTrans = all_panel[panelIndex].Find("Bingo");
            all_panel[panelIndex].DetachChildren();
            bingoTrans.SetParent(all_panel[panelIndex]);
            all_panel[panelIndex].sizeDelta = panelSize;
            all_panel[panelIndex].localPosition = panelPos[panelIndex];
            all_panel_bingo[panelIndex].gameObject.SetActive(false);
            all_panel_bingo[panelIndex].sizeDelta = BingoSize[gameType];
            all_panel_bingo[panelIndex].localPosition = BingoPos[gameType];
        }
        int currentPanelCount = all_panel.Count;
        for(int i = 0; i < currentPanelCount; i++)
        {
            if (i >= panelNum)
                all_panel[i].gameObject.SetActive(false);
        }
        
        for(int panelIndex = 0; panelIndex < panelNum; panelIndex++)
        {
            if (panelIndex > all_panel_cardItems.Count - 1)
                all_panel_cardItems.Add(new Card_Item[Columns_Count, Columns_Count]);
            for (int x = 0; x < Columns_Count; x++)
            {
                bool needWait = false;
                for (int y = 0; y < Columns_Count; y++)
                {
                    if (all_panel_cardItems[panelIndex][x, y] == null)
                    {
                        needWait = true;
                        Card_Item newItem = Instantiate(single_card.gameObject, all_panel[panelIndex]).GetComponent<Card_Item>();
                        all_panel_cardItems[panelIndex][x, y] = newItem;
                    }
                    int panelIndex_f = panelIndex;
                    all_panel_cardItems[panelIndex][x, y].Init(x, y, panelIndex_f, -1, false, Reward.Cash, cardNumTextSize, Reward.Empty);
                    RectTransform itemRect = all_panel_cardItems[panelIndex][x, y].Rect;
                    itemRect.SetParent(all_panel[panelIndex]);
                    itemRect.sizeDelta = cardSize;
                    itemRect.localPosition = new Vector3(-panelSize.x * 0.5f + horizontalInterval * x + 0.5f * itemRect.sizeDelta.x + leftOffset, panelSize.y * 0.5f - verticalInterval * y - 0.5f * itemRect.sizeDelta.y - upOffset);
                }
                if (needWait)
                    yield return null;
            }
        }
        #endregion

        List<Card_Bingo_Calculate[,]> preList;
        if (!isGuideStep1)
            preList = RandomGenerateCardNumIntoNextPool(panelNum, out ballNums, out notBingoCardPos, out noRewardCardPos, out hasRewardCardPos);
        else
        {
            preList = new List<Card_Bingo_Calculate[,]>();
            Card_Bingo_Calculate[,] fixCard = new Card_Bingo_Calculate[Columns_Count, Columns_Count]
            {
                {new Card_Bingo_Calculate(){num=8,reward=Reward.Cash},new Card_Bingo_Calculate(){num=4,reward=Reward.Empty},new Card_Bingo_Calculate(){num=14,reward=Reward.Gold},new Card_Bingo_Calculate(){num=5,reward=Reward.Cash},new Card_Bingo_Calculate(){num=9,reward=Reward.Cash}, },
                {new Card_Bingo_Calculate(){num=20,reward=Reward.Gold},new Card_Bingo_Calculate(){num=29,reward=Reward.Gold},new Card_Bingo_Calculate(){num=18,reward=Reward.Gold},new Card_Bingo_Calculate(){num=16,reward=Reward.Cash},new Card_Bingo_Calculate(){num=21,reward=Reward.Gold} },
                {new Card_Bingo_Calculate(){num=41,reward=Reward.Cash},new Card_Bingo_Calculate(){num=33,reward=Reward.Empty},new Card_Bingo_Calculate(){num=30,reward=Reward.Empty},new Card_Bingo_Calculate(){num=34,reward=Reward.Gold},new Card_Bingo_Calculate(){num=37,reward=Reward.Gold} },
                {new Card_Bingo_Calculate(){num=56,reward=Reward.Empty},new Card_Bingo_Calculate(){num=52,reward=Reward.Cash},new Card_Bingo_Calculate(){num=47,reward=Reward.Empty},new Card_Bingo_Calculate(){num=55,reward=Reward.Gold},new Card_Bingo_Calculate(){num=50,reward=Reward.Cash} },
                {new Card_Bingo_Calculate(){num=67,reward=Reward.Cash},new Card_Bingo_Calculate(){num=73,reward=Reward.Cash},new Card_Bingo_Calculate(){num=63,reward=Reward.Empty},new Card_Bingo_Calculate(){num=75,reward=Reward.Gold},new Card_Bingo_Calculate(){num=69,reward=Reward.Cash} }
            };
            preList.Add(fixCard);
            ballNums = new List<int>() { 8, 55, 69, 29, 9, 67, 16, 52, 26, 12, 21, 38, 34, 37, 18, 3, 75, 17, 54, 73, 53, 15, 50, 43, 4, 19, 1 };
        }
        all_panel_bingoCount.Clear();
        #region 赋予卡片数据
        all_panel_cardItem_Datas.Clear();
        int preListCount = preList.Count;
        for(int i = 0; i < preListCount; i++)
        {
            int xLength = preList[i].GetLength(0);
            int yLength = preList[i].GetLength(1);
            all_panel_cardItem_Datas.Add(new Card_ItemData[xLength, yLength]);
            all_panel_bingoCount.Add(0);
            for (int x = 0; x < xLength; x++)
            {
                for(int y = 0; y < yLength; y++)
                {
                    Card_ItemData itemData = new Card_ItemData()
                    {
                        num = preList[i][x, y].num,
                        hasOpen = false,
                        reward = preList[i][x, y].reward,
                        bingoReward = Reward.Empty
                    };
                    if (x == Columns_Count / 2 && y == Columns_Count / 2)
                        itemData.hasOpen = true;
                    all_panel_cardItem_Datas[i][x, y] = itemData;
                }
            }
        }
        #endregion

        ChangePage(1);
        GenerateBallCoroutine = StartCoroutine(GenerateNext(isGuideStep1, ballNums, false));
        if (!isGuideStep1)
        {
            extra_reward_progress_fillImage.transform.parent.gameObject.SetActive(true);
            StartCoroutine("AutoIncreaseExtraRewardProgress", panelNum);
        }
    }
    public bool ClickCard(Vector2Int pos,int panelIndex)
    {
        int num = all_panel_cardItem_Datas[panelIndex][pos.x, pos.y].num;
        foreach (var ball in hasShowBallNums)
            if (ball == num)
            {
                if (!all_panel_cardItem_Datas[panelIndex][pos.x, pos.y].hasOpen)
                {
                    all_panel_cardItem_Datas[panelIndex][pos.x, pos.y].hasOpen = true;
                    clickBingoCardTime++;
                    CheckBingo(pos, panelIndex);
                    if (!isGuideStep1)
                    {
                        if (all_panel_cardItem_Datas[panelIndex][pos.x, pos.y].reward != Reward.Empty)
                            hasRewardCardPos[panelIndex][pos.x].Remove(pos.y);
                        else
                            noRewardCardPos[panelIndex][pos.x].Remove(pos.y);
                    }
                    else if (UIManager.PanelShow(PopPanel.GuidePanel))
                    {
                        //UIManager.SendMessageToPanel((int)BasePanel.GamePanel, (int)PopPanel.GuidePanel);
                        //isPause = false;
                    }
                    return true;
                }
                return false;
            }

        return false;
    }
    public bool ForceBingoCard(Vector2Int pos,int panelIndex)
    {
        if (!all_panel_cardItem_Datas[panelIndex][pos.x, pos.y].hasOpen)
        {
            all_panel_cardItem_Datas[panelIndex][pos.x, pos.y].hasOpen = true;
            CheckBingo(pos, panelIndex);
            if (!isGuideStep1)
            {
                if (all_panel_cardItem_Datas[panelIndex][pos.x, pos.y].reward != Reward.Empty)
                    hasRewardCardPos[panelIndex][pos.x].Remove(pos.y);
                else
                    noRewardCardPos[panelIndex][pos.x].Remove(pos.y);
            }
            return true;
        }
        else
            return false;
    }
    public bool SetCardReward(Vector2Int pos, int panelIndex, Reward reward)
    {
        if (!all_panel_cardItem_Datas[panelIndex][pos.x, pos.y].hasOpen)
        {
            all_panel_cardItem_Datas[panelIndex][pos.x, pos.y].reward = reward;
            return true;
        }
        else
            return false;
    }
    public static int BingoCount = 0;
    private Vector2Int[] bingoRewardLockConfig = new Vector2Int[3]
    {
        new Vector2Int((int)BingoLockType.CashLock,50),
        new Vector2Int((int)BingoLockType.GoldLock,30),
        new Vector2Int((int)BingoLockType.StarLock,20)
    };
    public bool CheckBingo(Vector2Int pos,int panelIndex)
    {
        int columns = Columns_Count;
        bool isLeftup_rightdown = false;
        if (pos.x == pos.y)
            isLeftup_rightdown = true;
        bool isLeftdown_rightup = false;
        if (pos.x == columns - pos.y - 1)
            isLeftdown_rightup = true;
        int horizontalOpenCount = 0;
        int verticalOpenCount = 0;
        int leftup_rightdownOpenCount = 0;
        int leftdown_rightupOpenCount = 0;
        int cornerOpenCount = 0;
        bool h_key = false;
        bool v_key = false;
        bool s_a_key = false;
        bool s_b_key = false;
        bool corner_key = false;
        for (int i = 0; i < columns; i++)
        {
            Card_ItemData horizontalCard = all_panel_cardItem_Datas[panelIndex][pos.x, i];
            if (horizontalCard.hasOpen)
                horizontalOpenCount++;
            if (horizontalCard.reward == Reward.Key)
                h_key = true;
            Card_ItemData verticalCard = all_panel_cardItem_Datas[panelIndex][i, pos.y];
            if (verticalCard.hasOpen)
                verticalOpenCount++;
            if (verticalCard.reward == Reward.Key)
                v_key = true;
            if (isLeftup_rightdown)
            {
                Card_ItemData leftup_rightdownCard = all_panel_cardItem_Datas[panelIndex][i, i];
                if (leftup_rightdownCard.hasOpen)
                    leftup_rightdownOpenCount++;
                if (leftup_rightdownCard.reward == Reward.Key)
                    s_a_key = true;
            }
            if (isLeftdown_rightup)
            {
                Card_ItemData leftdown_rightupCard = all_panel_cardItem_Datas[panelIndex][i, columns - i - 1];
                if (leftdown_rightupCard.hasOpen)
                    leftdown_rightupOpenCount++;
                if (leftdown_rightupCard.reward == Reward.Key)
                    s_b_key = true;
            }
        }
        if (pos == new Vector2Int(0, 0) || pos == new Vector2Int(0, columns - 1) || pos == new Vector2Int(columns - 1, 0) || pos == new Vector2Int(columns - 1, columns - 1))
        {
            if (all_panel_cardItem_Datas[panelIndex][0, 0].hasOpen)
                cornerOpenCount++;
            if (all_panel_cardItem_Datas[panelIndex][0, 0].reward == Reward.Key)
                corner_key = true;
            if (all_panel_cardItem_Datas[panelIndex][0, columns - 1].hasOpen)
                cornerOpenCount++;
            if (all_panel_cardItem_Datas[panelIndex][0, columns - 1].reward == Reward.Key)
                corner_key = true;
            if (all_panel_cardItem_Datas[panelIndex][columns - 1, 0].hasOpen)
                cornerOpenCount++;
            if (all_panel_cardItem_Datas[panelIndex][columns - 1, 0].reward == Reward.Key)
                corner_key = true;
            if (all_panel_cardItem_Datas[panelIndex][columns - 1, columns - 1].hasOpen)
                cornerOpenCount++;
            if (all_panel_cardItem_Datas[panelIndex][columns - 1, columns - 1].reward == Reward.Key)
                corner_key = true;
        }
        int panelBingoCount = 0;
        if (horizontalOpenCount == columns)
        {
            panelBingoCount++;
            BingoLockType lockType;
            if (h_key)
                lockType = BingoLockType.KeyLock;
            else
                lockType = RandomBingoLockIcon();
            for (int i = 0; i < columns; i++)
                all_panel_cardItems[panelIndex][pos.x, i].SetBingoRewardLockImage(lockType);
            totalPigCashThisTime += ConfigManager.GetRandomPigCash(totalPigCashThisTime);
        }
        if (verticalOpenCount == columns)
        {
            panelBingoCount++;
            BingoLockType lockType;
            if (v_key)
                lockType = BingoLockType.KeyLock;
            else
                lockType = RandomBingoLockIcon();
            for (int i = 0; i < columns; i++)
                all_panel_cardItems[panelIndex][i, pos.y].SetBingoRewardLockImage(lockType);
            totalPigCashThisTime += ConfigManager.GetRandomPigCash(totalPigCashThisTime);
        }
        if (leftup_rightdownOpenCount == columns)
        {
            panelBingoCount++;
            BingoLockType lockType;
            if (s_a_key)
                lockType = BingoLockType.KeyLock;
            else
                lockType = RandomBingoLockIcon();
            for (int i = 0; i < columns; i++)
                all_panel_cardItems[panelIndex][i, i].SetBingoRewardLockImage(lockType);
            totalPigCashThisTime += ConfigManager.GetRandomPigCash(totalPigCashThisTime);
        }
        if (leftdown_rightupOpenCount == columns)
        {
            panelBingoCount++;
            BingoLockType lockType;
            if (s_b_key)
                lockType = BingoLockType.KeyLock;
            else
                lockType = RandomBingoLockIcon();
            for (int i = 0; i < columns; i++)
                all_panel_cardItems[panelIndex][i, columns - i - 1].SetBingoRewardLockImage(lockType);
            totalPigCashThisTime += ConfigManager.GetRandomPigCash(totalPigCashThisTime);
        }
        if (cornerOpenCount == 4)
        {
            panelBingoCount++;
            BingoLockType lockType;
            if (corner_key)
                lockType = BingoLockType.KeyLock;
            else
                lockType = RandomBingoLockIcon();
            all_panel_cardItems[panelIndex][0, 0].SetBingoRewardLockImage(lockType);
            all_panel_cardItems[panelIndex][0, columns - 1].SetBingoRewardLockImage(lockType);
            all_panel_cardItems[panelIndex][columns - 1, 0].SetBingoRewardLockImage(lockType);
            all_panel_cardItems[panelIndex][columns - 1, columns - 1].SetBingoRewardLockImage(lockType);
            totalPigCashThisTime += ConfigManager.GetRandomPigCash(totalPigCashThisTime);
        }
        all_panel_bingoCount[panelIndex] += panelBingoCount;
        BingoCount += panelBingoCount;
        if (BingoCount > 0)
        {
            isPause = true;
            if (showWheelWait == null && !UIManager.PanelShow(PopPanel.WheelPanel))
                showWheelWait = StartCoroutine(WaitToShowWheelPanel());
            if (panelBingoCount>0)
            {
                AudioManager.PlayOneShot(AudioPlayArea.Bingo);
                all_panel_bingo[panelIndex].SetAsLastSibling();
                all_panel_bingo[panelIndex].gameObject.SetActive(true);
            }
            return true;
        }
        return false;
    }
    public bool GetWillShowWheelPanel()
    {
        return showWheelWait != null;
    }
    public bool CheckBingoNeedLastone()
    {
        int panel = all_panel_cardItem_Datas.Count;
        for(int i = 0; i < panel; i++)
        {
            Card_ItemData[,] _ItemDatas = all_panel_cardItem_Datas[i];
            Data2Int[] openDatas = new Data2Int[Columns_Count];
            int s_aCount = 0;
            int s_bCount = 0;
            int cornerCount = 0;
            for(int x = 0; x < Columns_Count; x++)
            {
                for(int y = 0; y < Columns_Count; y++)
                {
                    if (_ItemDatas[x, y].hasOpen)
                    {
                        openDatas[x].data1++;
                        openDatas[y].data2++;
                        if (x == y)
                            s_aCount++;
                        if (x == Columns_Count - y - 1)
                            s_bCount++;
                        if (x == 0 && y == 0)
                            cornerCount++;
                        if (x == 0 && y == Columns_Count - 1)
                            cornerCount++;
                        if (x == Columns_Count - 1 && y == 0)
                            cornerCount++;
                        if (x == Columns_Count - 1 && y == Columns_Count - 1)
                            cornerCount++;
                    }
                }
            }
            if (s_aCount == 4 || s_bCount == 4)
                return true;
            if (cornerCount == 3)
                return true;
            foreach (var data in openDatas)
                if (data.data1 == 4 || data.data2 == 4)
                    return true;
        }
        return false;
    }
    Coroutine showWheelWait = null;
    private IEnumerator WaitToShowWheelPanel()
    {
        yield return new WaitForSeconds(2);
        UIManager.ShowPanel(PopPanel.WheelPanel);
        showWheelWait = null;
    }
    private BingoLockType RandomBingoLockIcon()
    {
        int total = 0;
        int configLength = bingoRewardLockConfig.Length;
        for(int i = 0; i < configLength; i++)
            total += bingoRewardLockConfig[i].y;
        int result = Random.Range(0, total);
        total = 0;
        for(int i = 0; i < configLength; i++)
        {
            total += bingoRewardLockConfig[i].y;
            if (result < total)
                return (BingoLockType)bingoRewardLockConfig[i].x;
        }

        throw new System.ArgumentException("bingo lock icon error");
    }
    public void AfterGetBingoWheelReward()
    {
        int panelCount = all_panel_bingoCount.Count;
        for(int i = 0; i < panelCount; i++)
        {
            if (all_panel_bingoCount[i] < 3)
            {
                all_panel_bingo[i].gameObject.SetActive(false);
                isPause = false;
            }
        }
        if (isPause)
            GameOver();
    }
    public void AfterCloseFirstGetExtraReward()
    {
        isPause = false;
    }
    private List<int> ballNums = new List<int>();
    private readonly List<int> hasShowBallNums = new List<int>();
    private readonly List<Card_Next> card_Nexts = new List<Card_Next>();
    private Vector3 oldBallScale = new Vector3(0.7820687f, 0.7820687f, 1);
    private Vector2 ballGeneratePos = new Vector2(85, 0);
    private int nextIndex = 0;
    private bool isPause = false;
    IEnumerator GenerateNext(bool inOrder,List<int> ballnums,bool isContinue)
    {
        List<int> allNum = ballnums;
        if (!isContinue)
        {
            hasShowBallNums.Clear();
            nextIndex = 0;
        }

        bool hasTriggerGuide = false;
        while (allNum.Count > 0)
        {
            if (isPause)
            {
                yield return null;
                continue;
            }
            Card_Next card_Next;
            int ballIndex = nextIndex % (maxShowNextCount + 1);
            int lastBallIndex = ballIndex - 1 < 0 ? maxShowNextCount : ballIndex - 1;
            if (ballIndex < card_Nexts.Count)
                card_Next = card_Nexts[ballIndex];
            else
            {
                card_Next = Instantiate(single_next, single_next.transform.parent).GetComponent<Card_Next>();
                card_Nexts.Add(card_Next);
            }
            card_Next.gameObject.SetActive(false);
            card_Next.Element.ignoreLayout = true;
            card_Next.Animator.enabled = true;
            card_Next.Rect.localPosition = ballGeneratePos;
            yield return null;
            card_Next.gameObject.SetActive(true);
            card_Next.Rect.SetAsFirstSibling();
            if (lastBallIndex <= card_Nexts.Count - 1)
            {
                card_Nexts[lastBallIndex].Animator.enabled = false;
                card_Nexts[lastBallIndex].Rect.localScale = oldBallScale;
                card_Nexts[lastBallIndex].Element.ignoreLayout = false;
            }

            int index;
            if (inOrder)
                index = 0;
            else
                index = Random.Range(0, allNum.Count);
            int num = allNum[index];
            AudioManager.PlayBallNum(num);
            allNum.Remove(num);
            hasShowBallNums.Add(num);
            ball_left_numText.text = allNum.Count.ToString();
            int charIndex = (num - 1) / NUM_PER_COLUMNS;
            char character = BINGO[charIndex];
            string color = BINGO_COLOR[charIndex];
            #region 提示未点击的格子
            int panelDataCount = all_panel_cardItem_Datas.Count;
            for(int panelIndex = 0; panelIndex < panelDataCount; panelIndex++)
            {
                Card_ItemData[,] panelData = all_panel_cardItem_Datas[panelIndex];
                int yLength = panelData.GetLength(1);
                for(int y = 0; y < yLength; y++)
                {
                    Card_ItemData data = panelData[charIndex, y];
                    if (!data.hasOpen && data.num == num)
                    {
                        all_panel_cardItems[panelIndex][charIndex, y].NoticeThisCard();
                        break;
                    }
                }
            }
            #endregion
            card_Next.Init(character.ToString(), num, SpriteManager.GetSprite(SpriteAtlas_Name.Game, color));
            if (isGuideStep1 && !hasTriggerGuide && !isContinue)
            {
                Vector3 cardWorldPos = all_panel_cardItems[0][0, 0].Rect.position;
                Vector2 cardSize = all_panel_cardItems[0][0, 0].Rect.sizeDelta;
                UIManager.ShowGuidePanel((int)BasePanel.GamePanel, cardWorldPos, cardSize, GuideType.GameGuide_ClickCard);
                isPause = true;
                hasTriggerGuide = true;
            }
            if (isGuideStep2 && !hasTriggerGuide && !isContinue)
            {
                Vector3 extraprogressWolrdPos = extra_rewardButton.transform.position;
                Vector2 size = extra_rewardButton.GetComponent<RectTransform>().sizeDelta;
                UIManager.ShowGuidePanel((int)BasePanel.GamePanel, extraprogressWolrdPos, size, GuideType.GameGuide_ExtraProp);
                isPause = true;
                hasTriggerGuide = true;
            }
            nextIndex++;
            float timer = 0;

            while (timer < generateNextInterval)
            {
                yield return null;
                if (!isPause)
                    timer += Time.deltaTime;
            }
        }
        GameOver();
    }
    private readonly List<Vector2Int> allGetRewards = new List<Vector2Int>();
    public void GainCardReward(Reward reward,int num)
    {
        int currentRewardCount = allGetRewards.Count;
        if (currentRewardCount == 0)
            allGetRewards.Add(new Vector2Int((int)reward, num));
        else
        {
            bool hasAdd = false;
            int rewardIndex = (int)reward;
            for(int i = 0; i < currentRewardCount; i++)
            {
                if (allGetRewards[i].x == rewardIndex)
                {
                    allGetRewards[i] = new Vector2Int(rewardIndex, allGetRewards[i].y + num);
                    hasAdd = true;
                    break;
                }
            }
            if(!hasAdd)
                allGetRewards.Add(new Vector2Int(rewardIndex, num));
        }
    }
    private void GameOver()
    {
        gameState = GameState.GameOver;
        AudioManager.PlayOneShot(AudioPlayArea.GameOver);
        animator.SetBool("GameOver", true);
    }
    bool hasGetMoreBall = false;
    public void OnGameOverAnimationEnd()
    {
        if (!hasGetMoreBall && !isPause)
        {
            int panelCount = all_panel_bingoCount.Count;
            bool cardAllBingo = true;
            for (int i = 0; i < panelCount; i++)
            {
                if (all_panel_bingoCount[i] < 3)
                {
                    all_panel_bingo[i].gameObject.SetActive(false);
                    cardAllBingo = false;
                    break;
                }
            }
            if (!cardAllBingo && CheckBingoNeedLastone() && SaveManager.SaveData.todayGetMoreBallTimes < ConfigManager.GetMoreBallChancePerDay)
            {
                hasGetMoreBall = true;
                UIManager.ShowPanel(PopPanel.SlotsPanel, (int)BasePanel.GamePanel, (int)Reward.Ball, 1, ConfigManager.GetRandomMoreBallNum());
                return;
            }
        }
        if (ConfigManager.CheckCanShowPig() && totalPigCashThisTime > 0)
            UIManager.ShowPanel(PopPanel.MoneyBoxPanel, (int)BasePanel.GamePanel, totalPigCashThisTime);
        else if(SaveManager.SaveData.cash_moneyBox>=ConfigManager.MoneyBoxCashTargetExchangeNeedNum)
            UIManager.ShowPanel(PopPanel.MoneyBoxPanel, (int)BasePanel.GamePanel, -1);
        else
            FinalGameOver();
    }
    private void FinalGameOver()
    {
        GameManager.Instance.TriggerTask(TaskType.PlayGame, 1);
        int argsCount = 1 + allGetRewards.Count * 2;
        int[] args = new int[argsCount];
        args[0] = all_panel_cardItem_Datas.Count;
        int currentRewardCount = allGetRewards.Count;
        for (int i = 0; i < currentRewardCount; i++)
        {
            int index = i;
            args[i * 2 + 1] = allGetRewards[index].x;
            args[i * 2 + 2] = allGetRewards[index].y;
        }
        GameManager.Instance.SendAdjustGameEndEvent(true, all_panel_cardItem_Datas.Count);
        GameManager.Instance.AddCompleteGameTime();
        if (args.Length > 1)
            UIManager.ShowPanel(PopPanel.GameOverPanel, args);
        else
        {
            UIManager.HidePanel(gameObject);
            UIManager.ShowPanel(BasePanel.MainPanel);
            UIManager.SendMessageToPanel(BasePanel.GamePanel, BasePanel.Menu, 0);
            AdsManager.HideBanner();
            generateCor = null;
        }
    }
    private List<Card_Bingo_Calculate[,]> RandomGenerateCardNumIntoNextPool(int panelNum,out List<int> ballNums,out List<List<List<int>>> notBingoCard_p_x_y,out List<List<List<int>>> noRewardCard_p_x_y,out List<List<List<int>>> hasRewardCard_p_x_y)
    {
        notBingoCard_p_x_y = new List<List<List<int>>>();
        noRewardCard_p_x_y = new List<List<List<int>>>();
        hasRewardCard_p_x_y = new List<List<List<int>>>();
        int cardSize = Columns_Count;
        int normalBingoNeedNum = cardSize;
        int cornerBingoNeedNum = 4;
        List<Card_Bingo_Calculate[,]> card_num_matrixs = new List<Card_Bingo_Calculate[,]>();
        List<List<int>> unUseNumPerColumns = new List<List<int>>();
        List<List<int>> notBallNumPerColumns = new List<List<int>>();
        List<List<int>> ballUseNumPerColumns = new List<List<int>>();
        for(int i = 0; i < panelNum; i++)
        {
            Card_Bingo_Calculate[,] matrix = new Card_Bingo_Calculate[cardSize, cardSize];
            int xLength = matrix.GetLength(0);
            int yLength = matrix.GetLength(1);
            for(int x = 0; x < xLength; x++)
            {
                for(int y = 0; y < yLength; y++)
                {
                    matrix[x, y] = new Card_Bingo_Calculate { horizontal = 0, vertical = 0, num = -1 };
                    if ((x == 0 && y == 0) || (x == cardSize - 1 && y == cardSize - 1))
                    {
                        matrix[x, y].corner = 0;
                        matrix[x, y].slopeA = 0;
                        matrix[x, y].slopeB = -1;
                    }
                    else if((x == 0 && y == cardSize - 1) || (x == cardSize - 1 && y == 0))
                    {
                        matrix[x, y].corner = 0;
                        matrix[x, y].slopeA = -1;
                        matrix[x, y].slopeB = 0;
                    }
                    else if (x == y)
                    {
                        matrix[x, y].slopeA = 0;
                        matrix[x, y].slopeB = -1;
                        matrix[x, y].corner = -1;
                    }
                    else if (x == cardSize - y - 1)
                    {
                        matrix[x, y].slopeA = -1;
                        matrix[x, y].slopeB = 0;
                        matrix[x, y].corner = -1;
                    }
                    else
                    {
                        matrix[x, y].slopeA = -1;
                        matrix[x, y].slopeB = -1;
                        matrix[x, y].corner = -1;
                    }
                    matrix[x, y].reward = Reward.Empty;
                }
            }
            if (cardSize <= 0 || cardSize % 2 == 0)
            {
                throw new System.FormatException("card size is not singular");
            }
            int maxUseNum = Random.Range(15, 19);
            if (i == 0)
            {
                List<List<int>> unEnsureCard = new List<List<int>>();
                List<List<int>> ensureCard = new List<List<int>>();//use to fill reward
                #region 设置中心点
                int centerX, centerY;
                centerX = centerY = cardSize / 2;
                int centerNum = Random.Range(NUM_PER_COLUMNS * centerX + 1, NUM_PER_COLUMNS * (centerX + 1) + 1);
                matrix[centerX, centerY].num = centerNum;
                for (int x = 0; x < cardSize; x++)
                {
                    int minNum = NUM_PER_COLUMNS * x + 1;
                    unUseNumPerColumns.Add(new List<int>());
                    ballUseNumPerColumns.Add(new List<int>());
                    for(int index = 0; index < NUM_PER_COLUMNS; index++)
                        unUseNumPerColumns[x].Add(minNum + index);
                    unEnsureCard.Add(new List<int>());
                    ensureCard.Add(new List<int>());
                    for (int y = 0; y < yLength; y++)
                        unEnsureCard[x].Add(y);

                    matrix[x, centerY].horizontal = 1;
                    matrix[centerX, x].vertical = 1;
                    matrix[x, x].slopeA = 1;
                    matrix[x, cardSize - x - 1].slopeB = 1;
                }
                unUseNumPerColumns[centerX].Remove(centerNum);
                ballUseNumPerColumns[centerX].Add(centerNum);
                unEnsureCard[centerX].Remove(centerY);
                maxUseNum--;
                #endregion
                #region 随机每列两个数字
                for (int x = 0; x < xLength; x++)
                {
                    List<int> unUseNum = unUseNumPerColumns[x];
                    int count = unUseNum.Count;
                    int num1 = unUseNum[Random.Range(0, count)];
                    unUseNum.Remove(num1);
                    ballUseNumPerColumns[x].Add(num1);
                    count--;
                    int num2 = unUseNum[Random.Range(0, count)];
                    unUseNum.Remove(num2);
                    ballUseNumPerColumns[x].Add(num2);

                    List<int> unUsePos = unEnsureCard[x];
                    int posCount = unUsePos.Count;
                    int pos1 = unUsePos[Random.Range(0, posCount)];
                    unUsePos.Remove(pos1);
                    ensureCard[x].Add(pos1);
                    posCount--;
                    int pos2 = unUsePos[Random.Range(0, posCount)];
                    unUsePos.Remove(pos2);
                    ensureCard[x].Add(pos2);
                    #region 设置第一个数
                    if (matrix[x, pos1].num == -1)
                    {
                        matrix[x, pos1].num = num1;
                        bool isSlopA = matrix[x, pos1].slopeA > -1;
                        bool isSlopB = matrix[x, pos1].slopeB > -1;
                        for(int index = 0; index < cardSize; index++)
                        {
                            matrix[index, pos1].horizontal++;
                            matrix[x, index].vertical++;
                            if (isSlopA)
                                matrix[index, index].slopeA++;
                            if (isSlopB)
                                matrix[index, cardSize - index - 1].slopeB++;
                        }
                        if (matrix[x, pos1].corner > -1)
                        {
                            matrix[0, 0].corner++;
                            matrix[0, cardSize - 1].corner++;
                            matrix[cardSize - 1, 0].corner++;
                            matrix[cardSize - 1, cardSize - 1].corner++;
                        }
                    }
                    else
                        throw new System.ArgumentException("card num has set");
                    #endregion
                    #region 设置第二个数
                    if (matrix[x, pos2].num == -1)
                    {
                        matrix[x, pos2].num = num2;
                        bool isSlopA = matrix[x, pos2].slopeA > -1;
                        bool isSlopB = matrix[x, pos2].slopeB > -1;
                        for (int index = 0; index < cardSize; index++)
                        {
                            matrix[index, pos2].horizontal++;
                            matrix[x, index].vertical++;
                            if (isSlopA)
                                matrix[index, index].slopeA++;
                            if (isSlopB)
                                matrix[index, cardSize - index - 1].slopeB++;
                        }
                        if (matrix[x, pos2].corner > -1)
                        {
                            matrix[0, 0].corner++;
                            matrix[0, cardSize - 1].corner++;
                            matrix[cardSize - 1, 0].corner++;
                            matrix[cardSize - 1, cardSize - 1].corner++;
                        }
                    }
                    else
                        throw new System.ArgumentException("card num has set");
                    #endregion
                    maxUseNum -= 2;
                }
                #endregion
                #region 计算bingo个数
                int bingoCount = CheckBingoCount(matrix);
                #endregion
                #region 判断是否需要填充bingo
                if (bingoCount < 1)
                {
                    int x;
                    int y;
                    while (true)
                    {
                        x = Random.Range(0, unEnsureCard.Count);
                        if (unEnsureCard[x].Count > 0)
                        {
                            List<int> yList = unEnsureCard[x];
                            y = yList[Random.Range(0, yList.Count)];
                            break;
                        }
                    }
                    Card_Bingo_Calculate _Calculate = matrix[x, y];
                    int horizontal_need = normalBingoNeedNum - _Calculate.horizontal;
                    int vertical_need = normalBingoNeedNum - _Calculate.vertical;
                    int slopeA_need = 999;
                    int slopeB_need = 999;
                    int corner_need = 999;
                    if (_Calculate.slopeA > -1)
                        slopeA_need = normalBingoNeedNum - _Calculate.slopeA;
                    else if (_Calculate.slopeB > -1)
                        slopeB_need = normalBingoNeedNum - _Calculate.slopeB;
                    if (_Calculate.corner > -1)
                        corner_need = cornerBingoNeedNum - _Calculate.corner;
                    #region 填充空格子来获得bingo
                    int min_need = Mathf.Min(horizontal_need, vertical_need, slopeA_need, slopeB_need, corner_need);
                    if (min_need == horizontal_need)
                    {
                        for (int xIndex = 0; xIndex < xLength; xIndex++)
                        {
                            if (matrix[xIndex, y].num == -1)
                            {
                                List<int> columns_unuse_num = unUseNumPerColumns[xIndex];
                                int num = columns_unuse_num[Random.Range(0, columns_unuse_num.Count)];
                                columns_unuse_num.Remove(num);
                                ballUseNumPerColumns[xIndex].Add(num);
                                matrix[xIndex, y].num = num;
                                bool isSlopA = matrix[xIndex, y].slopeA > -1;
                                bool isSlopB = matrix[xIndex, y].slopeB > -1;
                                for (int index = 0; index < cardSize; index++)
                                {
                                    matrix[index, y].horizontal++;
                                    matrix[xIndex, index].vertical++;
                                    if (isSlopA)
                                        matrix[index, index].slopeA++;
                                    if (isSlopB)
                                        matrix[index, cardSize - index - 1].slopeB++;
                                }
                                if (matrix[xIndex, y].corner > -1)
                                {
                                    matrix[0, 0].corner++;
                                    matrix[0, cardSize - 1].corner++;
                                    matrix[cardSize - 1, 0].corner++;
                                    matrix[cardSize - 1, cardSize - 1].corner++;
                                }
                                unEnsureCard[xIndex].Remove(y);
                                ensureCard[xIndex].Add(y);
                                maxUseNum--;
                            }
                        }
                    }
                    else if (min_need == vertical_need)
                    {
                        for (int yIndex = 0; yIndex < xLength; yIndex++)
                        {
                            if (matrix[x, yIndex].num == -1)
                            {
                                List<int> columns_unuse_num = unUseNumPerColumns[x];
                                int num = columns_unuse_num[Random.Range(0, columns_unuse_num.Count)];
                                columns_unuse_num.Remove(num);
                                ballUseNumPerColumns[x].Add(num);
                                matrix[x, yIndex].num = num;
                                bool isSlopA = matrix[x, yIndex].slopeA > -1;
                                bool isSlopB = matrix[x, yIndex].slopeB > -1;
                                for (int index = 0; index < cardSize; index++)
                                {
                                    matrix[index, yIndex].horizontal++;
                                    matrix[x, index].vertical++;
                                    if (isSlopA)
                                        matrix[index, index].slopeA++;
                                    if (isSlopB)
                                        matrix[index, cardSize - index - 1].slopeB++;
                                }
                                if (matrix[x, yIndex].corner > -1)
                                {
                                    matrix[0, 0].corner++;
                                    matrix[0, cardSize - 1].corner++;
                                    matrix[cardSize - 1, 0].corner++;
                                    matrix[cardSize - 1, cardSize - 1].corner++;
                                }
                                unEnsureCard[x].Remove(yIndex);
                                ensureCard[x].Add(yIndex);
                                maxUseNum--;
                            }
                        }
                    }
                    else if (min_need == slopeA_need)
                    {
                        for (int xIndex = 0; xIndex < xLength; xIndex++)
                        {
                            if (matrix[xIndex, xIndex].num == -1)
                            {
                                List<int> columns_unuse_num = unUseNumPerColumns[xIndex];
                                int num = columns_unuse_num[Random.Range(0, columns_unuse_num.Count)];
                                columns_unuse_num.Remove(num);
                                ballUseNumPerColumns[xIndex].Add(num);
                                matrix[xIndex, xIndex].num = num;
                                for (int index = 0; index < cardSize; index++)
                                {
                                    matrix[index, xIndex].horizontal++;
                                    matrix[xIndex, index].vertical++;
                                    matrix[index, index].slopeA++;
                                }
                                if (matrix[xIndex, xIndex].corner > -1)
                                {
                                    matrix[0, 0].corner++;
                                    matrix[0, cardSize - 1].corner++;
                                    matrix[cardSize - 1, 0].corner++;
                                    matrix[cardSize - 1, cardSize - 1].corner++;
                                }
                                unEnsureCard[xIndex].Remove(xIndex);
                                ensureCard[xIndex].Add(xIndex);
                                maxUseNum--;
                            }
                        }
                    }
                    else if (min_need == slopeB_need)
                    {
                        for (int xIndex = 0; xIndex < xLength; xIndex++)
                        {
                            if (matrix[xIndex, cardSize - xIndex - 1].num == -1)
                            {
                                List<int> columns_unuse_num = unUseNumPerColumns[xIndex];
                                int num = columns_unuse_num[Random.Range(0, columns_unuse_num.Count)];
                                columns_unuse_num.Remove(num);
                                ballUseNumPerColumns[xIndex].Add(num);
                                matrix[xIndex, cardSize - xIndex - 1].num = num;
                                for (int index = 0; index < cardSize; index++)
                                {
                                    matrix[index, cardSize - xIndex - 1].horizontal++;
                                    matrix[xIndex, index].vertical++;
                                    matrix[index, cardSize - index - 1].slopeB++;
                                }
                                if (matrix[xIndex, cardSize - xIndex - 1].corner > -1)
                                {
                                    matrix[0, 0].corner++;
                                    matrix[0, cardSize - 1].corner++;
                                    matrix[cardSize - 1, 0].corner++;
                                    matrix[cardSize - 1, cardSize - 1].corner++;
                                }
                                unEnsureCard[xIndex].Remove(cardSize - xIndex - 1);
                                ensureCard[xIndex].Add(cardSize - xIndex - 1);
                                maxUseNum--;
                            }
                        }
                    }
                    else if (min_need == corner_need)
                    {
                        if (matrix[0, 0].num == -1)
                        {
                            List<int> columns_unuse_num = unUseNumPerColumns[0];
                            int num = columns_unuse_num[Random.Range(0, columns_unuse_num.Count)];
                            columns_unuse_num.Remove(num);
                            ballUseNumPerColumns[0].Add(num);
                            matrix[0, 0].num = num;
                            for (int index = 0; index < cardSize; index++)
                            {
                                matrix[index, 0].horizontal++;
                                matrix[0, index].vertical++;
                                matrix[index, index].slopeA++;
                            }
                            matrix[0, 0].corner++;
                            matrix[0, cardSize - 1].corner++;
                            matrix[cardSize - 1, 0].corner++;
                            matrix[cardSize - 1, cardSize - 1].corner++;
                            maxUseNum--;
                            unEnsureCard[0].Remove(0);
                            ensureCard[0].Add(0);
                        }
                        if (matrix[0, cardSize - 1].num == -1)
                        {
                            List<int> columns_unuse_num = unUseNumPerColumns[0];
                            int num = columns_unuse_num[Random.Range(0, columns_unuse_num.Count)];
                            columns_unuse_num.Remove(num);
                            ballUseNumPerColumns[0].Add(num);
                            matrix[0, cardSize - 1].num = num;
                            for (int index = 0; index < cardSize; index++)
                            {
                                matrix[index, cardSize - 1].horizontal++;
                                matrix[0, index].vertical++;
                                matrix[index, cardSize - index - 1].slopeB++;
                            }
                            matrix[0, 0].corner++;
                            matrix[0, cardSize - 1].corner++;
                            matrix[cardSize - 1, 0].corner++;
                            matrix[cardSize - 1, cardSize - 1].corner++;
                            maxUseNum--;
                            unEnsureCard[0].Remove(cardSize - 1);
                            ensureCard[0].Add(cardSize - 1);
                        }
                        if (matrix[cardSize - 1, 0].num == -1)
                        {
                            List<int> columns_unuse_num = unUseNumPerColumns[cardSize - 1];
                            int num = columns_unuse_num[Random.Range(0, columns_unuse_num.Count)];
                            columns_unuse_num.Remove(num);
                            ballUseNumPerColumns[cardSize - 1].Add(num);
                            matrix[cardSize - 1, 0].num = num;
                            for (int index = 0; index < cardSize; index++)
                            {
                                matrix[index, 0].horizontal++;
                                matrix[cardSize - 1, index].vertical++;
                                matrix[index, cardSize - index - 1].slopeB++;
                            }
                            matrix[0, 0].corner++;
                            matrix[0, cardSize - 1].corner++;
                            matrix[cardSize - 1, 0].corner++;
                            matrix[cardSize - 1, cardSize - 1].corner++;
                            maxUseNum--;
                            unEnsureCard[cardSize - 1].Remove(0);
                            ensureCard[cardSize - 1].Add(0);
                        }
                        if (matrix[cardSize - 1, cardSize - 1].num == -1)
                        {
                            List<int> columns_unuse_num = unUseNumPerColumns[cardSize - 1];
                            int num = columns_unuse_num[Random.Range(0, columns_unuse_num.Count)];
                            columns_unuse_num.Remove(num);
                            ballUseNumPerColumns[cardSize - 1].Add(num);
                            matrix[cardSize - 1, cardSize - 1].num = num;
                            for (int index = 0; index < cardSize; index++)
                            {
                                matrix[index, cardSize - 1].horizontal++;
                                matrix[cardSize - 1, index].vertical++;
                                matrix[index, index].slopeA++;
                            }
                            matrix[0, 0].corner++;
                            matrix[0, cardSize - 1].corner++;
                            matrix[cardSize - 1, 0].corner++;
                            matrix[cardSize - 1, cardSize - 1].corner++;
                            maxUseNum--;
                            unEnsureCard[cardSize - 1].Remove(cardSize - 1);
                            ensureCard[cardSize - 1].Add(cardSize - 1);
                        }
                    }
                    #endregion
                }
                #endregion
                #region 填充剩余需要的格子
                for (int index = maxUseNum; index > 0; index--)
                {
                    int x = -1;
                    int y = -1;
                    List<List<int>> cloneUnEnsureCard = CloneDoubleNestList(unEnsureCard);
                    int count = GetDoubleNestListCount(cloneUnEnsureCard);
                    int findTimer = 0;
                    while (count > 0)
                    {
                        findTimer++;
                        int tempX = Random.Range(0, cloneUnEnsureCard.Count);
                        List<int> unEnsureColumnCard = cloneUnEnsureCard[tempX];
                        if (unEnsureColumnCard.Count <= 0)
                            continue;
                        else
                        {
                            count--;
                            int tempY = unEnsureColumnCard[Random.Range(0, unEnsureColumnCard.Count)];
                            unEnsureColumnCard.Remove(tempY);
                            Card_Bingo_Calculate _Calculate = matrix[tempX, tempY];
                            if (_Calculate.num == -1)
                            {
                                if (_Calculate.horizontal >= normalBingoNeedNum - 1)
                                    continue;
                                if (_Calculate.vertical >= normalBingoNeedNum - 1)
                                    continue;
                                if (_Calculate.slopeA >= normalBingoNeedNum - 1)
                                    continue;
                                if (_Calculate.slopeB >= normalBingoNeedNum - 1)
                                    continue;
                                if (_Calculate.corner >= cornerBingoNeedNum - 1)
                                    continue;
                                x = tempX;
                                y = tempY;
                                break;
                            }
                            else
                                continue;
                        }
                    }
                    if (count <= 0)
                    {
                        if (x == -1 || y == -1)
                            break;
                    }
                    else
                    {
                        List<int> unUseNum = unUseNumPerColumns[x];
                        int num = unUseNum[Random.Range(0, unUseNum.Count)];
                        unUseNum.Remove(num);
                        ballUseNumPerColumns[x].Add(num);
                        if (matrix[x, y].num == -1)
                        {
                            matrix[x, y].num = num;
                            bool isSlopA = matrix[x, y].slopeA > -1;
                            bool isSlopB = matrix[x, y].slopeB > -1;
                            for (int j = 0; j < cardSize; j++)
                            {
                                matrix[j, y].horizontal++;
                                matrix[x, j].vertical++;
                                if (isSlopA)
                                    matrix[j, j].slopeA++;
                                if (isSlopB)
                                    matrix[j, cardSize - j - 1].slopeB++;
                            }
                            if (matrix[x, y].corner > -1)
                            {
                                matrix[0, 0].corner++;
                                matrix[0, cardSize - 1].corner++;
                                matrix[cardSize - 1, 0].corner++;
                                matrix[cardSize - 1, cardSize - 1].corner++;
                            }
                            maxUseNum--;
                            unEnsureCard[x].Remove(y);
                            ensureCard[x].Add(y);
                        }
                        else
                            throw new System.ArgumentException("card num has set");
                    }
                }
                #endregion
                notBallNumPerColumns = CloneDoubleNestList(unUseNumPerColumns);
                notBingoCard_p_x_y.Add(CloneDoubleNestList(unEnsureCard));
                List<List<int>> hasRewardCard = new List<List<int>>();
                List<List<int>> noRewardCard = new List<List<int>>();
                for(int x = 0; x < xLength; x++)
                {
                    hasRewardCard.Add(new List<int>());
                    noRewardCard.Add(new List<int>());
                    for (int y = 0; y < yLength; y++)
                        noRewardCard[x].Add(y);
                }
                noRewardCard[centerX].Remove(centerY);
                #region 填充金币奖励
                int goldCardNum = ConfigManager.GetCardRandomRewardCardNum(Reward.Gold);
                int bingoCardGoldNum = Mathf.Min(goldCardNum, MaxBingoGoldReward);
                for(int goldCardIndex = 0; goldCardIndex < bingoCardGoldNum; goldCardIndex++)
                {
                    int goldCard_x;
                    int goldCard_y;
                    while (true)
                    {
                        goldCard_x = Random.Range(0, ensureCard.Count);
                        List<int> yIndexList = ensureCard[goldCard_x];
                        if (yIndexList.Count > 0)
                        {
                            goldCard_y = yIndexList[Random.Range(0, yIndexList.Count)];
                            yIndexList.Remove(goldCard_y);
                            break;
                        }
                    }
                    matrix[goldCard_x, goldCard_y].reward = Reward.Gold;
                    hasRewardCard[goldCard_x].Add(goldCard_y);
                    noRewardCard[goldCard_x].Remove(goldCard_y);
                }
                #endregion
                #region 填充现金奖励
                int cashCardNum = ConfigManager.GetCardRandomRewardCardNum(Reward.Cash);
                int bingoCardCashNum = Mathf.Min(cashCardNum, MaxBingoCashReward);
                for (int cashCardIndex = 0; cashCardIndex < bingoCardCashNum; cashCardIndex++)
                {
                    int cashCard_x;
                    int cashCard_y;
                    while (true)
                    {
                        cashCard_x = Random.Range(0, ensureCard.Count);
                        List<int> yIndexList = ensureCard[cashCard_x];
                        if (yIndexList.Count > 0)
                        {
                            cashCard_y = yIndexList[Random.Range(0, yIndexList.Count)];
                            yIndexList.Remove(cashCard_y);
                            break;
                        }
                    }
                    matrix[cashCard_x, cashCard_y].reward = Reward.Cash;
                    hasRewardCard[cashCard_x].Add(cashCard_y);
                    noRewardCard[cashCard_x].Remove(cashCard_y);
                }
                #endregion
                #region 填充钥匙奖励
                bool keyIsDouble = Random.Range(0, 100) >= 70;
                int keyCard_x;
                int keyCard_y;
                while (true)
                {
                    keyCard_x = Random.Range(0, ensureCard.Count);
                    List<int> yIndexList = ensureCard[keyCard_x];
                    if (yIndexList.Count > 0)
                    {
                        keyCard_y = yIndexList[Random.Range(0, yIndexList.Count)];
                        yIndexList.Remove(keyCard_y);
                        break;
                    }
                }
                matrix[keyCard_x, keyCard_y].reward = Reward.Key;
                hasRewardCard[keyCard_x].Add(keyCard_y);
                noRewardCard[keyCard_x].Remove(keyCard_y);
                #endregion
                #region 填充多余奖励到无法点击的格子
                ensureCard = CloneDoubleNestList(unEnsureCard);
                int oddGoldCard = goldCardNum - bingoCardGoldNum;
                if(oddGoldCard>0)
                    for(int goldCardIndex = 0; goldCardIndex < oddGoldCard; goldCardIndex++)
                    {
                        int goldCard_x;
                        int goldCard_y;
                        while (true)
                        {
                            goldCard_x = Random.Range(0, ensureCard.Count);
                            List<int> yIndexList = ensureCard[goldCard_x];
                            if (yIndexList.Count > 0)
                            {
                                goldCard_y = yIndexList[Random.Range(0, yIndexList.Count)];
                                yIndexList.Remove(goldCard_y);
                                break;
                            }
                        }
                        matrix[goldCard_x, goldCard_y].reward = Reward.Gold;
                        hasRewardCard[goldCard_x].Add(goldCard_y);
                        noRewardCard[goldCard_x].Remove(goldCard_y);
                    }
                int oddCashCard = cashCardNum - bingoCardCashNum;
                if (oddCashCard > 0) 
                    for(int cashCardIndex = 0; cashCardIndex < oddCashCard; cashCardIndex++)
                    {
                        int cashCard_x;
                        int cashCard_y;
                        while (true)
                        {
                            cashCard_x = Random.Range(0, ensureCard.Count);
                            List<int> yIndexList = ensureCard[cashCard_x];
                            if (yIndexList.Count > 0)
                            {
                                cashCard_y = yIndexList[Random.Range(0, yIndexList.Count)];
                                yIndexList.Remove(cashCard_y);
                                break;
                            }
                        }
                        matrix[cashCard_x, cashCard_y].reward = Reward.Cash;
                        hasRewardCard[cashCard_x].Add(cashCard_y);
                        noRewardCard[cashCard_x].Remove(cashCard_y);
                    }
                if (keyIsDouble)
                {
                    int keyCard_x2;
                    int keyCard_y2;
                    while (true)
                    {
                        keyCard_x2 = Random.Range(0, ensureCard.Count);
                        List<int> yIndexList = ensureCard[keyCard_x2];
                        if (yIndexList.Count > 0)
                        {
                            keyCard_y2 = yIndexList[Random.Range(0, yIndexList.Count)];
                            yIndexList.Remove(keyCard_y2);
                            break;
                        }
                    }
                    matrix[keyCard_x2, keyCard_y2].reward = Reward.Key;
                    hasRewardCard[keyCard_x2].Add(keyCard_y2);
                    noRewardCard[keyCard_x2].Remove(keyCard_y2);
                }
                #endregion
                hasRewardCard_p_x_y.Add(hasRewardCard);
                noRewardCard_p_x_y.Add(noRewardCard);
                #region 填充无法点击的格子
                int unEnsureCard_columnCount = unEnsureCard.Count;
                for (int x = 0; x < unEnsureCard_columnCount; x++)
                {
                    int unEnsureCard_rowCount = unEnsureCard[x].Count;
                    if (unEnsureCard[x].Count <= 0)
                        continue;
                    else
                    {
                        for (int yIndex = 0; yIndex < unEnsureCard_rowCount; yIndex++)
                        {
                            int y = unEnsureCard[x][yIndex];
                            List<int> columnsNums = unUseNumPerColumns[x];
                            int num = columnsNums[Random.Range(0, columnsNums.Count)];
                            columnsNums.Remove(num);
                            matrix[x, y].num = num;
                        }
                    }
                }
                #endregion
            }
            else//后续bingo板，可以点击的选取前面板子上面没有的数字或者已经确定的球的数字，不可点击的选取非球的数字
            {
                List<List<int>> unEnsureCard = new List<List<int>>();
                List<List<int>> unUseBallList = CloneDoubleNestList(ballUseNumPerColumns);
                List<List<int>> ensureCard = new List<List<int>>();//use to fill reward
                int ballCount = GetDoubleNestListCount(ballUseNumPerColumns);
                #region 设置中心点
                int centerX, centerY;
                centerX = centerY = cardSize / 2;
                int centerNum;
                if (ballCount >= MaxBallCount)
                {
                    List<int> ballColumnsList = unUseBallList[centerX];
                    centerNum = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                    ballColumnsList.Remove(centerNum);
                }
                else
                {
                    List<int> columnsList = unUseNumPerColumns[centerX];
                    if (columnsList.Count > 0)
                    {
                        centerNum = columnsList[Random.Range(0, columnsList.Count)];
                        columnsList.Remove(centerNum);
                        notBallNumPerColumns[centerX].Remove(centerNum);
                        ballUseNumPerColumns[centerX].Add(centerNum);
                    }
                    else
                    {
                        List<int> ballColumnsList = unUseBallList[centerX];
                        centerNum = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                        ballColumnsList.Remove(centerNum);
                    }
                }
                matrix[centerX, centerY].num = centerNum;
                for (int x = 0; x < cardSize; x++)
                {
                    unEnsureCard.Add(new List<int>());
                    ensureCard.Add(new List<int>());
                    for (int y = 0; y < yLength; y++)
                        unEnsureCard[x].Add(y);

                    matrix[x, centerY].horizontal = 1;
                    matrix[centerX, x].vertical = 1;
                    matrix[x, x].slopeA = 1;
                    matrix[x, cardSize - x - 1].slopeB = 1;
                }
                unEnsureCard[centerX].Remove(centerY);
                maxUseNum--;
                #endregion
                #region 随机每列两个数字
                for(int x = 0; x < xLength; x++)
                {
                    ballCount = GetDoubleNestListCount(ballUseNumPerColumns);
                    int num1;
                    #region 选取第一个数字
                    if (ballCount >= MaxBallCount)
                    {
                        List<int> ballColumnsList = unUseBallList[x];
                        num1 = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                        ballColumnsList.Remove(num1);
                    }
                    else
                    {
                        List<int> unUseColumnsList = unUseNumPerColumns[x];
                        if (unUseNumPerColumns.Count > 0)
                        {
                            num1 = unUseColumnsList[Random.Range(0, unUseColumnsList.Count)];
                            unUseColumnsList.Remove(num1);
                            notBallNumPerColumns[x].Remove(num1);
                            ballUseNumPerColumns[x].Add(num1);
                        }
                        else
                        {
                            List<int> ballColumnsList = unUseBallList[x];
                            num1 = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                            ballColumnsList.Remove(num1);
                        }
                    }
                    #endregion
                    #region 选取第二个数字
                    ballCount = GetDoubleNestListCount(ballUseNumPerColumns);
                    int num2;
                    if (ballCount >= MaxBallCount)
                    {
                        List<int> ballColumnsList = unUseBallList[x];
                        num2 = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                        ballColumnsList.Remove(num2);
                    }
                    else
                    {
                        List<int> unUseColumnsList = unUseNumPerColumns[x];
                        if (unUseNumPerColumns.Count > 0)
                        {
                            num2 = unUseColumnsList[Random.Range(0, unUseColumnsList.Count)];
                            unUseColumnsList.Remove(num2);
                            notBallNumPerColumns[x].Remove(num2);
                            ballUseNumPerColumns[x].Add(num2);
                        }
                        else
                        {
                            List<int> ballColumnsList = unUseBallList[x];
                            num2 = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                            ballColumnsList.Remove(num2);
                        }
                    }
                    #endregion
                    List<int> unUsePos = unEnsureCard[x];
                    int posCount = unUsePos.Count;
                    int pos1 = unUsePos[Random.Range(0, posCount)];
                    unUsePos.Remove(pos1);
                    ensureCard[x].Add(pos1);
                    posCount--;
                    int pos2 = unUsePos[Random.Range(0, posCount)];
                    unUsePos.Remove(pos2);
                    ensureCard[x].Add(pos2);
                    #region 设置第一个数
                    if (matrix[x, pos1].num == -1)
                    {
                        matrix[x, pos1].num = num1;
                        bool isSlopA = matrix[x, pos1].slopeA > -1;
                        bool isSlopB = matrix[x, pos1].slopeB > -1;
                        for (int index = 0; index < cardSize; index++)
                        {
                            matrix[index, pos1].horizontal++;
                            matrix[x, index].vertical++;
                            if (isSlopA)
                                matrix[index, index].slopeA++;
                            if (isSlopB)
                                matrix[index, cardSize - index - 1].slopeB++;
                        }
                        if (matrix[x, pos1].corner > -1)
                        {
                            matrix[0, 0].corner++;
                            matrix[0, cardSize - 1].corner++;
                            matrix[cardSize - 1, 0].corner++;
                            matrix[cardSize - 1, cardSize - 1].corner++;
                        }
                    }
                    else
                        throw new System.ArgumentException("card num has set");
                    #endregion
                    #region 设置第二个数
                    if (matrix[x, pos2].num == -1)
                    {
                        matrix[x, pos2].num = num2;
                        bool isSlopA = matrix[x, pos2].slopeA > -1;
                        bool isSlopB = matrix[x, pos2].slopeB > -1;
                        for (int index = 0; index < cardSize; index++)
                        {
                            matrix[index, pos2].horizontal++;
                            matrix[x, index].vertical++;
                            if (isSlopA)
                                matrix[index, index].slopeA++;
                            if (isSlopB)
                                matrix[index, cardSize - index - 1].slopeB++;
                        }
                        if (matrix[x, pos2].corner > -1)
                        {
                            matrix[0, 0].corner++;
                            matrix[0, cardSize - 1].corner++;
                            matrix[cardSize - 1, 0].corner++;
                            matrix[cardSize - 1, cardSize - 1].corner++;
                        }
                    }
                    else
                        throw new System.ArgumentException("card num has set");
                    #endregion
                    maxUseNum -= 2;
                }
                #endregion
                int bingoCount = CheckBingoCount(matrix);
                #region 填充一个bingo需要的格子
                if (bingoCount < 1)
                {
                    int x = -1;
                    int y = -1;
                    BingoFillType fillType = BingoFillType.Null;
                    int findCount = 0;
                    bool hasFind = false;
                    List<List<int>> cloneUnEnsureCard = CloneDoubleNestList(unEnsureCard);
                    int count = GetDoubleNestListCount(cloneUnEnsureCard);
                    while (count>0)
                    {
                        findCount++;
                        x = Random.Range(0, cloneUnEnsureCard.Count);
                        List<int> yList = cloneUnEnsureCard[x];
                        if (yList.Count > 0)
                        {
                            y = yList[Random.Range(0, yList.Count)];
                            yList.Remove(y);
                            count--;
                            Card_Bingo_Calculate _Calculate = matrix[x, y];
                            int horizontal_need = normalBingoNeedNum - _Calculate.horizontal;
                            int vertical_need = normalBingoNeedNum - _Calculate.vertical;
                            int slopeA_need = 999;
                            int slopeB_need = 999;
                            int corner_need = 999;
                            if (_Calculate.slopeA > -1)
                                slopeA_need = normalBingoNeedNum - _Calculate.slopeA;
                            else if (_Calculate.slopeB > -1)
                                slopeB_need = normalBingoNeedNum - _Calculate.slopeB;
                            if (_Calculate.corner > -1)
                                corner_need = cornerBingoNeedNum - _Calculate.corner;
                            ballCount = GetDoubleNestListCount(ballUseNumPerColumns);
                            #region 检测横向是否可以填充
                            bool h_canFill = true;
                            int h_needFillNum = 0;
                            for (int xIndex = 0; xIndex < xLength; xIndex++)
                            {
                                if (matrix[xIndex, y].num == -1)
                                {
                                    h_needFillNum++;
                                    if (ballCount + h_needFillNum - 1 >= MaxBallCount)
                                    {
                                        List<int> ballColumnsList = unUseBallList[xIndex];
                                        if (ballColumnsList.Count <= 0)
                                        {
                                            h_canFill = false;
                                            break;
                                        }
                                        else
                                            continue;
                                    }
                                    else
                                    {
                                        List<int> unUseColumnsList = unUseNumPerColumns[xIndex];
                                        if (unUseColumnsList.Count > 0)
                                            continue;
                                        else
                                        {
                                            List<int> ballColumnsList = unUseBallList[xIndex];
                                            if (ballColumnsList.Count <= 0)
                                            {
                                                h_canFill = false;
                                                break;
                                            }
                                            else
                                                continue;
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region 检测纵向是否可以填充
                            int v_needFillNum = 0;
                            bool v_canFill = true;
                            for (int yIndex = 0; yIndex < xLength; yIndex++)
                            {
                                if (matrix[x, yIndex].num == -1)
                                {
                                    v_needFillNum++;
                                    if (ballCount + v_needFillNum - 1 >= MaxBallCount)
                                    {
                                        List<int> ballColumnsList = unUseBallList[x];
                                        if (ballColumnsList.Count < v_needFillNum)
                                        {
                                            v_canFill = false;
                                            break;
                                        }
                                        else
                                            continue;
                                    }
                                    else
                                    {
                                        List<int> unUseColumnsList = unUseNumPerColumns[x];
                                        if (unUseColumnsList.Count >= v_needFillNum)
                                            continue;
                                        else
                                        {
                                            List<int> ballColumnsList = unUseBallList[x];
                                            if (ballColumnsList.Count < v_needFillNum)
                                            {
                                                v_canFill = false;
                                                break;
                                            }
                                            else
                                                continue;
                                        }
                                    }
                                }
                            }
                            #endregion
                            bool sA_canFill = true;
                            if (slopeA_need != 999)
                            {
                                #region 检测斜向A是否可以填充
                                int sA_needFillNum = 0;
                                for (int xIndex = 0; xIndex < xLength; xIndex++)
                                {
                                    if (matrix[xIndex, xIndex].num == -1)
                                    {
                                        sA_needFillNum++;
                                        if (ballCount + sA_needFillNum - 1 >= MaxBallCount)
                                        {
                                            List<int> ballColumnsList = unUseBallList[xIndex];
                                            if (ballColumnsList.Count <= 0)
                                            {
                                                sA_canFill = false;
                                                break;
                                            }
                                            else
                                                continue;
                                        }
                                        else
                                        {
                                            List<int> unUseColumnsList = unUseNumPerColumns[xIndex];
                                            if (unUseColumnsList.Count > 0)
                                                continue;
                                            else
                                            {
                                                List<int> ballColumnsList = unUseBallList[xIndex];
                                                if (ballColumnsList.Count <= 0)
                                                {
                                                    sA_canFill = false;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                            else
                                sA_canFill = false;
                            bool sB_canFill = true;
                            if (slopeB_need != 999)
                            {
                                #region 检测斜向B是否可以填充
                                int sB_needFillNum = 0;
                                for (int xIndex = 0; xIndex < xLength; xIndex++)
                                {
                                    if (matrix[xIndex, cardSize - xIndex - 1].num == -1)
                                    {
                                        sB_needFillNum++;
                                        if (ballCount + sB_needFillNum - 1 >= MaxBallCount)
                                        {
                                            List<int> ballColumnsList = unUseBallList[xIndex];
                                            if (ballColumnsList.Count <= 0)
                                            {
                                                sB_canFill = false;
                                                break;
                                            }
                                            else
                                                continue;
                                        }
                                        else
                                        {
                                            List<int> unUseColumnsList = unUseNumPerColumns[xIndex];
                                            if (unUseColumnsList.Count > 0)
                                                continue;
                                            else
                                            {
                                                List<int> ballColumnsList = unUseBallList[xIndex];
                                                if (ballColumnsList.Count <= 0)
                                                {
                                                    sB_canFill = false;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                            else
                                sB_canFill = false;
                            bool corner_canFill = true;
                            if (corner_need != 999)
                            {
                                #region 检测四角是否可以填充
                                Card_Bingo_Calculate corner1 = matrix[0, 0];
                                Card_Bingo_Calculate corner2 = matrix[0, cardSize - 1];
                                Card_Bingo_Calculate corner3 = matrix[cardSize - 1, 0];
                                Card_Bingo_Calculate corner4 = matrix[cardSize - 1, cardSize - 1];
                                int needFillNum_start = 0;
                                int needFillNum_end = 0;
                                if (corner1.num == -1)
                                {
                                    needFillNum_start++;
                                    if (ballCount + needFillNum_start - 1 >= MaxBallCount)
                                    {
                                        List<int> ballColumnsList = unUseBallList[0];
                                        if (ballColumnsList.Count < needFillNum_start)
                                            corner_canFill = false;
                                    }
                                    else
                                    {
                                        List<int> unUseColumnsList = unUseNumPerColumns[0];
                                        if (unUseColumnsList.Count < needFillNum_start)
                                        {
                                            List<int> ballColumnsList = unUseBallList[0];
                                            if (ballColumnsList.Count < needFillNum_start)
                                                corner_canFill = false;
                                        }
                                    }
                                }
                                if (corner2.num == -1)
                                {
                                    needFillNum_start++;
                                    if (ballCount + needFillNum_start - 1 >= MaxBallCount)
                                    {
                                        List<int> ballColumnsList = unUseBallList[0];
                                        if (ballColumnsList.Count < needFillNum_start)
                                            corner_canFill = false;
                                    }
                                    else
                                    {
                                        List<int> unUseColumnsList = unUseNumPerColumns[0];
                                        if (unUseColumnsList.Count < needFillNum_start)
                                        {
                                            List<int> ballColumnsList = unUseBallList[0];
                                            if (ballColumnsList.Count < needFillNum_start)
                                                corner_canFill = false;
                                        }
                                    }
                                }
                                if (corner3.num == -1)
                                {
                                    needFillNum_end++;
                                    if (ballCount + needFillNum_end - 1 >= MaxBallCount)
                                    {
                                        List<int> ballColumnsList = unUseBallList[cardSize - 1];
                                        if (ballColumnsList.Count < needFillNum_end)
                                            corner_canFill = false;
                                    }
                                    else
                                    {
                                        List<int> unUseColumnsList = unUseNumPerColumns[cardSize - 1];
                                        if (unUseColumnsList.Count < needFillNum_end)
                                        {
                                            List<int> ballColumnsList = unUseBallList[cardSize - 1];
                                            if (ballColumnsList.Count < needFillNum_end)
                                                corner_canFill = false;
                                        }
                                    }
                                }
                                if (corner4.num == -1)
                                {
                                    needFillNum_end++;
                                    if (ballCount + needFillNum_end - 1 >= MaxBallCount)
                                    {
                                        List<int> ballColumnsList = unUseBallList[cardSize - 1];
                                        if (ballColumnsList.Count < needFillNum_end)
                                            corner_canFill = false;
                                    }
                                    else
                                    {
                                        List<int> unUseColumnsList = unUseNumPerColumns[cardSize - 1];
                                        if (unUseColumnsList.Count < needFillNum_end)
                                        {
                                            List<int> ballColumnsList = unUseBallList[cardSize - 1];
                                            if (ballColumnsList.Count < needFillNum_end)
                                                corner_canFill = false;
                                        }
                                    }
                                }
                                #endregion
                            }
                            else
                                corner_canFill = false;
                            #region 判断填充类型
                            List<Vector2Int> bingo_need = new List<Vector2Int>();
                            if (h_canFill)
                                bingo_need.Add(new Vector2Int((int)BingoFillType.Horizontal, horizontal_need));
                            if (v_canFill)
                                bingo_need.Add(new Vector2Int((int)BingoFillType.Vertical, vertical_need));
                            if (sA_canFill)
                                bingo_need.Add(new Vector2Int((int)BingoFillType.SlopeA, slopeA_need));
                            if (sB_canFill)
                                bingo_need.Add(new Vector2Int((int)BingoFillType.SlopeB, slopeB_need));
                            if (corner_canFill)
                                bingo_need.Add(new Vector2Int((int)BingoFillType.Corner, corner_need));

                            int minFillNeed = 999;
                            int minfillType = -1;
                            for(int fillTypeIndex = 0; fillTypeIndex < bingo_need.Count; fillTypeIndex++)
                            {
                                Vector2Int fillState = bingo_need[fillTypeIndex];
                                if (fillState.y < minFillNeed)
                                    minfillType = fillState.x;
                            }
                            if (minfillType == -1)
                                continue;
                            fillType = (BingoFillType)minfillType;
                            if (fillType != BingoFillType.Null)
                            {
                                hasFind = true;
                                break;
                            }
                            #endregion
                        }
                    }
                    if (count <= 0)
                    {
                        if (!hasFind)
                            throw new System.ArgumentOutOfRangeException("fill bingo error : not enought num to fill");
                    }
                    #region 根据填充类型开始填充
                    switch (fillType)
                    {
                        case BingoFillType.Horizontal:
                            for (int xIndex = 0; xIndex < xLength; xIndex++)
                            {
                                if (matrix[xIndex, y].num == -1)
                                {
                                    ballCount = GetDoubleNestListCount(ballUseNumPerColumns);
                                    int num;
                                    if (ballCount >= MaxBallCount)
                                    {
                                        List<int> ballColumnsList = unUseBallList[xIndex];
                                        num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                        ballColumnsList.Remove(num);
                                    }
                                    else
                                    {
                                        List<int> unUseColumnsList = unUseNumPerColumns[xIndex];
                                        if (unUseColumnsList.Count > 0)
                                        {
                                            num = unUseColumnsList[Random.Range(0, unUseColumnsList.Count)];
                                            unUseColumnsList.Remove(num);
                                            notBallNumPerColumns[xIndex].Remove(num);
                                            ballUseNumPerColumns[xIndex].Add(num);
                                        }
                                        else
                                        {
                                            List<int> ballColumnsList = unUseBallList[xIndex];
                                            num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                            ballColumnsList.Remove(num);
                                        }
                                    }
                                    matrix[xIndex, y].num = num;
                                    bool isSlopA = matrix[xIndex, y].slopeA > -1;
                                    bool isSlopB = matrix[xIndex, y].slopeB > -1;
                                    for (int index = 0; index < cardSize; index++)
                                    {
                                        matrix[index, y].horizontal++;
                                        matrix[xIndex, index].vertical++;
                                        if (isSlopA)
                                            matrix[index, index].slopeA++;
                                        if (isSlopB)
                                            matrix[index, cardSize - index - 1].slopeB++;
                                    }
                                    if (matrix[xIndex, y].corner > -1)
                                    {
                                        matrix[0, 0].corner++;
                                        matrix[0, cardSize - 1].corner++;
                                        matrix[cardSize - 1, 0].corner++;
                                        matrix[cardSize - 1, cardSize - 1].corner++;
                                    }
                                    maxUseNum--;
                                    unEnsureCard[xIndex].Remove(y);
                                    ensureCard[xIndex].Add(y);
                                }
                            }
                            break;
                        case BingoFillType.Vertical:
                            for (int yIndex = 0; yIndex < xLength; yIndex++)
                            {
                                if (matrix[x, yIndex].num == -1)
                                {
                                    ballCount = GetDoubleNestListCount(ballUseNumPerColumns);
                                    int num;
                                    if (ballCount >= MaxBallCount)
                                    {
                                        List<int> ballColumnsList = unUseBallList[x];
                                        num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                        ballColumnsList.Remove(num);
                                    }
                                    else
                                    {
                                        List<int> unUseColumnsList = unUseNumPerColumns[x];
                                        if (unUseColumnsList.Count > 0)
                                        {
                                            num = unUseColumnsList[Random.Range(0, unUseColumnsList.Count)];
                                            unUseColumnsList.Remove(num);
                                            notBallNumPerColumns[x].Remove(num);
                                            ballUseNumPerColumns[x].Add(num);
                                        }
                                        else
                                        {
                                            List<int> ballColumnsList = unUseBallList[x];
                                            num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                            ballColumnsList.Remove(num);
                                        }
                                    }
                                    matrix[x, yIndex].num = num;
                                    bool isSlopA = matrix[x, yIndex].slopeA > -1;
                                    bool isSlopB = matrix[x, yIndex].slopeB > -1;
                                    for (int index = 0; index < cardSize; index++)
                                    {
                                        matrix[index, yIndex].horizontal++;
                                        matrix[x, index].vertical++;
                                        if (isSlopA)
                                            matrix[index, index].slopeA++;
                                        if (isSlopB)
                                            matrix[index, cardSize - index - 1].slopeB++;
                                    }
                                    if (matrix[x, yIndex].corner > -1)
                                    {
                                        matrix[0, 0].corner++;
                                        matrix[0, cardSize - 1].corner++;
                                        matrix[cardSize - 1, 0].corner++;
                                        matrix[cardSize - 1, cardSize - 1].corner++;
                                    }
                                    maxUseNum--;
                                    unEnsureCard[x].Remove(yIndex);
                                    ensureCard[x].Add(yIndex);
                                }
                            }
                            break;
                        case BingoFillType.SlopeA:
                            for (int xIndex = 0; xIndex < xLength; xIndex++)
                            {
                                if (matrix[xIndex, xIndex].num == -1)
                                {
                                    ballCount = GetDoubleNestListCount(ballUseNumPerColumns);
                                    int num;
                                    if (ballCount >= MaxBallCount)
                                    {
                                        List<int> ballColumnsList = unUseBallList[xIndex];
                                        num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                        ballColumnsList.Remove(num);
                                    }
                                    else
                                    {
                                        List<int> unUseColumnsList = unUseNumPerColumns[xIndex];
                                        if (unUseColumnsList.Count > 0)
                                        {
                                            num = unUseColumnsList[Random.Range(0, unUseColumnsList.Count)];
                                            unUseColumnsList.Remove(num);
                                            notBallNumPerColumns[xIndex].Remove(num);
                                            ballUseNumPerColumns[xIndex].Add(num);
                                        }
                                        else
                                        {
                                            List<int> ballColumnsList = unUseBallList[xIndex];
                                            num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                            ballColumnsList.Remove(num);
                                        }
                                    }
                                    matrix[xIndex, xIndex].num = num;
                                    for (int index = 0; index < cardSize; index++)
                                    {
                                        matrix[index, xIndex].horizontal++;
                                        matrix[xIndex, index].vertical++;
                                        matrix[index, index].slopeA++;
                                    }
                                    if (matrix[xIndex, xIndex].corner > -1)
                                    {
                                        matrix[0, 0].corner++;
                                        matrix[0, cardSize - 1].corner++;
                                        matrix[cardSize - 1, 0].corner++;
                                        matrix[cardSize - 1, cardSize - 1].corner++;
                                    }
                                    unEnsureCard[xIndex].Remove(xIndex);
                                    ensureCard[xIndex].Add(xIndex);
                                    maxUseNum--;
                                }
                            }
                            break;
                        case BingoFillType.SlopeB:
                            for (int xIndex = 0; xIndex < xLength; xIndex++)
                            {
                                if (matrix[xIndex, cardSize - xIndex - 1].num == -1)
                                {
                                    ballCount = GetDoubleNestListCount(ballUseNumPerColumns);
                                    int num;
                                    if (ballCount >= MaxBallCount)
                                    {
                                        List<int> ballColumnsList = unUseBallList[xIndex];
                                        num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                        ballColumnsList.Remove(num);
                                    }
                                    else
                                    {
                                        List<int> unUseColumnsList = unUseNumPerColumns[xIndex];
                                        if (unUseColumnsList.Count > 0)
                                        {
                                            num = unUseColumnsList[Random.Range(0, unUseColumnsList.Count)];
                                            unUseColumnsList.Remove(num);
                                            notBallNumPerColumns[xIndex].Remove(num);
                                            ballUseNumPerColumns[xIndex].Add(num);
                                        }
                                        else
                                        {
                                            List<int> ballColumnsList = unUseBallList[xIndex];
                                            num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                            ballColumnsList.Remove(num);
                                        }
                                    }
                                    matrix[xIndex, cardSize - xIndex - 1].num = num;
                                    for (int index = 0; index < cardSize; index++)
                                    {
                                        matrix[index, cardSize - xIndex - 1].horizontal++;
                                        matrix[xIndex, index].vertical++;
                                        matrix[index, cardSize - index - 1].slopeB++;
                                    }
                                    if (matrix[xIndex, cardSize - xIndex - 1].corner > -1)
                                    {
                                        matrix[0, 0].corner++;
                                        matrix[0, cardSize - 1].corner++;
                                        matrix[cardSize - 1, 0].corner++;
                                        matrix[cardSize - 1, cardSize - 1].corner++;
                                    }
                                    maxUseNum--;
                                    unEnsureCard[xIndex].Remove(cardSize - xIndex - 1);
                                    ensureCard[xIndex].Add(cardSize - xIndex - 1);
                                }
                            }
                            break;
                        case BingoFillType.Corner:
                            if (matrix[0, 0].num == -1)
                            {
                                ballCount = GetDoubleNestListCount(ballUseNumPerColumns);
                                int num;
                                if (ballCount >= MaxBallCount)
                                {
                                    List<int> ballColumnsList = unUseBallList[0];
                                    num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                    ballColumnsList.Remove(num);
                                }
                                else
                                {
                                    List<int> unUseColumnsList = unUseNumPerColumns[0];
                                    if (unUseColumnsList.Count > 0)
                                    {
                                        num = unUseColumnsList[Random.Range(0, unUseColumnsList.Count)];
                                        unUseColumnsList.Remove(num);
                                        notBallNumPerColumns[0].Remove(num);
                                        ballUseNumPerColumns[0].Add(num);
                                    }
                                    else
                                    {
                                        List<int> ballColumnsList = unUseBallList[0];
                                        num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                        ballColumnsList.Remove(num);
                                    }
                                }
                                matrix[0, 0].num = num;
                                for (int index = 0; index < cardSize; index++)
                                {
                                    matrix[index, 0].horizontal++;
                                    matrix[0, index].vertical++;
                                    matrix[index, index].slopeA++;
                                }
                                matrix[0, 0].corner++;
                                matrix[0, cardSize - 1].corner++;
                                matrix[cardSize - 1, 0].corner++;
                                matrix[cardSize - 1, cardSize - 1].corner++;
                                maxUseNum--;
                                unEnsureCard[0].Remove(0);
                                ensureCard[0].Add(0);
                            }
                            if (matrix[0, cardSize - 1].num == -1)
                            {
                                ballCount = GetDoubleNestListCount(ballUseNumPerColumns);
                                int num;
                                if (ballCount >= MaxBallCount)
                                {
                                    List<int> ballColumnsList = unUseBallList[0];
                                    num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                    ballColumnsList.Remove(num);
                                }
                                else
                                {
                                    List<int> unUseColumnsList = unUseNumPerColumns[0];
                                    if (unUseColumnsList.Count > 0)
                                    {
                                        num = unUseColumnsList[Random.Range(0, unUseColumnsList.Count)];
                                        unUseColumnsList.Remove(num);
                                        notBallNumPerColumns[0].Remove(num);
                                        ballUseNumPerColumns[0].Add(num);
                                    }
                                    else
                                    {
                                        List<int> ballColumnsList = unUseBallList[0];
                                        num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                        ballColumnsList.Remove(num);
                                    }
                                }
                                matrix[0, cardSize - 1].num = num;
                                for (int index = 0; index < cardSize; index++)
                                {
                                    matrix[index, cardSize - 1].horizontal++;
                                    matrix[0, index].vertical++;
                                    matrix[index, cardSize - index - 1].slopeB++;
                                }
                                matrix[0, 0].corner++;
                                matrix[0, cardSize - 1].corner++;
                                matrix[cardSize - 1, 0].corner++;
                                matrix[cardSize - 1, cardSize - 1].corner++;
                                maxUseNum--;
                                unEnsureCard[0].Remove(cardSize - 1);
                                ensureCard[0].Add(cardSize - 1);
                            }
                            if (matrix[cardSize - 1, 0].num == -1)
                            {
                                ballCount = GetDoubleNestListCount(ballUseNumPerColumns);
                                int num;
                                if (ballCount >= MaxBallCount)
                                {
                                    List<int> ballColumnsList = unUseBallList[cardSize - 1];
                                    num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                    ballColumnsList.Remove(num);
                                }
                                else
                                {
                                    List<int> unUseColumnsList = unUseNumPerColumns[cardSize - 1];
                                    if (unUseColumnsList.Count > 0)
                                    {
                                        num = unUseColumnsList[Random.Range(0, unUseColumnsList.Count)];
                                        unUseColumnsList.Remove(num);
                                        notBallNumPerColumns[cardSize - 1].Remove(num);
                                        ballUseNumPerColumns[cardSize - 1].Add(num);
                                    }
                                    else
                                    {
                                        List<int> ballColumnsList = unUseBallList[cardSize - 1];
                                        num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                        ballColumnsList.Remove(num);
                                    }
                                }
                                matrix[cardSize - 1, 0].num = num;
                                for (int index = 0; index < cardSize; index++)
                                {
                                    matrix[index, 0].horizontal++;
                                    matrix[cardSize - 1, index].vertical++;
                                    matrix[index, cardSize - index - 1].slopeB++;
                                }
                                matrix[0, 0].corner++;
                                matrix[0, cardSize - 1].corner++;
                                matrix[cardSize - 1, 0].corner++;
                                matrix[cardSize - 1, cardSize - 1].corner++;
                                maxUseNum--;
                                unEnsureCard[cardSize - 1].Remove(0);
                                ensureCard[cardSize - 1].Add(0);
                            }
                            if (matrix[cardSize - 1, cardSize - 1].num == -1)
                            {
                                ballCount = GetDoubleNestListCount(ballUseNumPerColumns);
                                int num;
                                if (ballCount >= MaxBallCount)
                                {
                                    List<int> ballColumnsList = unUseBallList[cardSize - 1];
                                    num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                    ballColumnsList.Remove(num);
                                }
                                else
                                {
                                    List<int> unUseColumnsList = unUseNumPerColumns[cardSize - 1];
                                    if (unUseColumnsList.Count > 0)
                                    {
                                        num = unUseColumnsList[Random.Range(0, unUseColumnsList.Count)];
                                        unUseColumnsList.Remove(num);
                                        notBallNumPerColumns[cardSize - 1].Remove(num);
                                        ballUseNumPerColumns[cardSize - 1].Add(num);
                                    }
                                    else
                                    {
                                        List<int> ballColumnsList = unUseBallList[cardSize - 1];
                                        num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                        ballColumnsList.Remove(num);
                                    }
                                }
                                matrix[cardSize - 1, cardSize - 1].num = num;
                                for (int index = 0; index < cardSize; index++)
                                {
                                    matrix[index, cardSize - 1].horizontal++;
                                    matrix[cardSize - 1, index].vertical++;
                                    matrix[index, index].slopeA++;
                                }
                                matrix[0, 0].corner++;
                                matrix[0, cardSize - 1].corner++;
                                matrix[cardSize - 1, 0].corner++;
                                matrix[cardSize - 1, cardSize - 1].corner++;
                                maxUseNum--;
                                unEnsureCard[cardSize - 1].Remove(cardSize - 1);
                                ensureCard[cardSize - 1].Add(cardSize - 1);
                            }
                            break;
                        default:
                            break;
                    }
                    #endregion
                }
                #endregion
                #region 填充剩余需要的格子
                ballCount = GetDoubleNestListCount(ballUseNumPerColumns);
                List<List<int>> cloneUnEnsureCard_ForFillExtra = CloneDoubleNestList(unEnsureCard);
                int count_ForFillExtra = GetDoubleNestListCount(cloneUnEnsureCard_ForFillExtra);
                for (int index = maxUseNum; index > 0; index--)
                {
                    if (count_ForFillExtra <= 0)
                    {
                        Debug.LogError("can not find fit num to fill card");
                        break;
                    }
                    int x = -1;
                    int y = -1;
                    int findTimer = 0;
                    while (count_ForFillExtra > 0)
                    {
                        findTimer++;
                        int tempX = Random.Range(0, cloneUnEnsureCard_ForFillExtra.Count);
                        List<int> unEnsureColumnCard = cloneUnEnsureCard_ForFillExtra[tempX];
                        if (unEnsureColumnCard.Count <= 0)
                            continue;
                        else
                        {
                            count_ForFillExtra--;
                            int tempY = unEnsureColumnCard[Random.Range(0, unEnsureColumnCard.Count)];
                            cloneUnEnsureCard_ForFillExtra[tempX].Remove(tempY);
                            //Debug.Log(tempX + "," + tempY + "/" + index);
                            Card_Bingo_Calculate _Calculate = matrix[tempX, tempY];
                            if (_Calculate.num == -1)
                            {
                                if (_Calculate.horizontal >= normalBingoNeedNum - 1)
                                    continue;
                                if (_Calculate.vertical >= normalBingoNeedNum - 1)
                                    continue;
                                if (_Calculate.slopeA >= normalBingoNeedNum - 1)
                                    continue;
                                if (_Calculate.slopeB >= normalBingoNeedNum - 1)
                                    continue;
                                if (_Calculate.corner >= cornerBingoNeedNum - 1)
                                    continue;
                                if (ballCount + maxUseNum - index >= MaxBallCount)
                                {
                                    List<int> ballColumnsList = unUseBallList[tempX];
                                    if (ballColumnsList.Count <= 0)
                                        continue;
                                }
                                else
                                {
                                    List<int> unUseColumnList = unUseNumPerColumns[tempX];
                                    if (unUseColumnList.Count <= 0)
                                    {
                                        List<int> ballColumnsList = unUseBallList[tempX];
                                        if (ballColumnsList.Count <= 0)
                                            continue;
                                    }
                                }
                                x = tempX;
                                y = tempY;
                                break;
                            }
                            else
                                continue;
                        }
                    }
                    if (count_ForFillExtra <= 0)
                    {
                        if (x == -1 || y == -1)
                        {
                            Debug.LogWarning(index + " can not fill");
                            break;
                            List<int> balls = ConvertDoubleNestListToOneList(ballUseNumPerColumns);
                            System.Text.StringBuilder sb = new System.Text.StringBuilder();
                            for(int ballIndex = 0; ballIndex < balls.Count; ballIndex++)
                            {
                                sb.Append(balls[ballIndex] + ", ");
                            }
                            Debug.LogError(sb.ToString());
                        }
                    }
                    else
                    {
                        int num;
                        if (ballCount + maxUseNum - index >= MaxBallCount)
                        {
                            List<int> ballColumnsList = unUseBallList[x];
                            if (ballColumnsList.Count <= 0)
                            {
                                index++;
                                Debug.LogError("?");
                                continue;
                            }
                            num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                            ballColumnsList.Remove(num);
                        }
                        else
                        {
                            List<int> unUseColumnList = unUseNumPerColumns[x];
                            if (unUseColumnList.Count > 0)
                            {
                                num = unUseColumnList[Random.Range(0, unUseColumnList.Count)];
                                unUseColumnList.Remove(num);
                                notBallNumPerColumns[x].Remove(num);
                                ballUseNumPerColumns[x].Add(num);
                            }
                            else
                            {
                                List<int> ballColumnsList = unUseBallList[x];
                                if (ballColumnsList.Count <= 0)
                                {
                                    index++;
                                    continue;
                                }
                                num = ballColumnsList[Random.Range(0, ballColumnsList.Count)];
                                ballColumnsList.Remove(num);
                            }
                        }
                        if (matrix[x, y].num == -1)
                        {
                            unEnsureCard[x].Remove(y);
                            ensureCard[x].Add(y);
                            matrix[x, y].num = num;
                            bool isSlopA = matrix[x, y].slopeA > -1;
                            bool isSlopB = matrix[x, y].slopeB > -1;
                            for (int j = 0; j < cardSize; j++)
                            {
                                matrix[j, y].horizontal++;
                                matrix[x, j].vertical++;
                                if (isSlopA)
                                    matrix[j, j].slopeA++;
                                if (isSlopB)
                                    matrix[j, cardSize - j - 1].slopeB++;
                            }
                            if (matrix[x, y].corner > -1)
                            {
                                matrix[0, 0].corner++;
                                matrix[0, cardSize - 1].corner++;
                                matrix[cardSize - 1, 0].corner++;
                                matrix[cardSize - 1, cardSize - 1].corner++;
                            }
                        }
                        else
                            throw new System.ArgumentException("card num has set");
                    }
                }
                #endregion
                notBingoCard_p_x_y.Add(CloneDoubleNestList(unEnsureCard));
                List<List<int>> hasRewardCard = new List<List<int>>();
                List<List<int>> noRewardCard = new List<List<int>>();
                for (int x = 0; x < xLength; x++)
                {
                    hasRewardCard.Add(new List<int>());
                    noRewardCard.Add(new List<int>());
                    for (int y = 0; y < yLength; y++)
                        noRewardCard[x].Add(y);
                }
                noRewardCard[centerX].Remove(centerY);
                #region 填充金币奖励
                int goldCardNum = ConfigManager.GetCardRandomRewardCardNum(Reward.Gold);
                int bingoCardGoldNum = Mathf.Min(goldCardNum, MaxBingoGoldReward);
                for (int goldCardIndex = 0; goldCardIndex < bingoCardGoldNum; goldCardIndex++)
                {
                    int goldCard_x;
                    int goldCard_y;
                    while (true)
                    {
                        goldCard_x = Random.Range(0, ensureCard.Count);
                        List<int> yIndexList = ensureCard[goldCard_x];
                        if (yIndexList.Count > 0)
                        {
                            goldCard_y = yIndexList[Random.Range(0, yIndexList.Count)];
                            yIndexList.Remove(goldCard_y);
                            break;
                        }
                    }
                    matrix[goldCard_x, goldCard_y].reward = Reward.Gold;
                    hasRewardCard[goldCard_x].Add(goldCard_y);
                    noRewardCard[goldCard_x].Remove(goldCard_y);
                }
                #endregion
                #region 填充现金奖励
                int cashCardNum = ConfigManager.GetCardRandomRewardCardNum(Reward.Cash);
                int bingoCardCashNum = Mathf.Min(cashCardNum, MaxBingoCashReward);
                for (int cashCardIndex = 0; cashCardIndex < bingoCardCashNum; cashCardIndex++)
                {
                    int cashCard_x;
                    int cashCard_y;
                    while (true)
                    {
                        cashCard_x = Random.Range(0, ensureCard.Count);
                        List<int> yIndexList = ensureCard[cashCard_x];
                        if (yIndexList.Count > 0)
                        {
                            cashCard_y = yIndexList[Random.Range(0, yIndexList.Count)];
                            yIndexList.Remove(cashCard_y);
                            break;
                        }
                    }
                    matrix[cashCard_x, cashCard_y].reward = Reward.Cash;
                    hasRewardCard[cashCard_x].Add(cashCard_y);
                    noRewardCard[cashCard_x].Remove(cashCard_y);
                }
                #endregion
                #region 填充钥匙奖励
                bool keyIsDouble = Random.Range(0, 100) >= 70;
                int keyCard_x;
                int keyCard_y;
                while (true)
                {
                    keyCard_x = Random.Range(0, ensureCard.Count);
                    List<int> yIndexList = ensureCard[keyCard_x];
                    if (yIndexList.Count > 0)
                    {
                        keyCard_y = yIndexList[Random.Range(0, yIndexList.Count)];
                        yIndexList.Remove(keyCard_y);
                        break;
                    }
                }
                matrix[keyCard_x, keyCard_y].reward = Reward.Key;
                hasRewardCard[keyCard_x].Add(keyCard_y);
                noRewardCard[keyCard_x].Remove(keyCard_y);
                #endregion
                #region 填充多余奖励到无法点击的格子
                ensureCard = CloneDoubleNestList(unEnsureCard);
                int oddGoldCard = goldCardNum - bingoCardGoldNum;
                if (oddGoldCard > 0)
                    for (int goldCardIndex = 0; goldCardIndex < oddGoldCard; goldCardIndex++)
                    {
                        int goldCard_x;
                        int goldCard_y;
                        while (true)
                        {
                            goldCard_x = Random.Range(0, ensureCard.Count);
                            List<int> yIndexList = ensureCard[goldCard_x];
                            if (yIndexList.Count > 0)
                            {
                                goldCard_y = yIndexList[Random.Range(0, yIndexList.Count)];
                                yIndexList.Remove(goldCard_y);
                                break;
                            }
                        }
                        matrix[goldCard_x, goldCard_y].reward = Reward.Gold;
                        hasRewardCard[goldCard_x].Add(goldCard_y);
                        noRewardCard[goldCard_x].Remove(goldCard_y);
                    }
                int oddCashCard = cashCardNum - bingoCardCashNum;
                if (oddCashCard > 0)
                    for (int cashCardIndex = 0; cashCardIndex < oddCashCard; cashCardIndex++)
                    {
                        int cashCard_x;
                        int cashCard_y;
                        while (true)
                        {
                            cashCard_x = Random.Range(0, ensureCard.Count);
                            List<int> yIndexList = ensureCard[cashCard_x];
                            if (yIndexList.Count > 0)
                            {
                                cashCard_y = yIndexList[Random.Range(0, yIndexList.Count)];
                                yIndexList.Remove(cashCard_y);
                                break;
                            }
                        }
                        matrix[cashCard_x, cashCard_y].reward = Reward.Cash;
                        hasRewardCard[cashCard_x].Add(cashCard_y);
                        noRewardCard[cashCard_x].Remove(cashCard_y);
                    }
                if (keyIsDouble)
                {
                    int keyCard_x2;
                    int keyCard_y2;
                    while (true)
                    {
                        keyCard_x2 = Random.Range(0, ensureCard.Count);
                        List<int> yIndexList = ensureCard[keyCard_x2];
                        if (yIndexList.Count > 0)
                        {
                            keyCard_y2 = yIndexList[Random.Range(0, yIndexList.Count)];
                            yIndexList.Remove(keyCard_y2);
                            break;
                        }
                    }
                    matrix[keyCard_x2, keyCard_y2].reward = Reward.Key;
                    hasRewardCard[keyCard_x2].Add(keyCard_y2);
                    noRewardCard[keyCard_x2].Remove(keyCard_y2);
                }
                #endregion
                hasRewardCard_p_x_y.Add(hasRewardCard);
                noRewardCard_p_x_y.Add(noRewardCard);
                #region 填充无法点击的格子
                int unEnsureCard_columnCount = unEnsureCard.Count;
                List<List<int>> cloneUnUseNotBallNum = CloneDoubleNestList(notBallNumPerColumns);
                for (int x = 0; x < unEnsureCard_columnCount; x++)
                {
                    int unEnsureCard_rowCount = unEnsureCard[x].Count;
                    if (unEnsureCard[x].Count <= 0)
                        continue;
                    else
                    {
                        for (int yIndex = 0; yIndex < unEnsureCard_rowCount; yIndex++)
                        {
                            int y = unEnsureCard[x][yIndex];
                            List<int> columnsNums = cloneUnUseNotBallNum[x];
                            if (columnsNums.Count > 0)
                            {
                                int num = columnsNums[Random.Range(0, columnsNums.Count)];
                                columnsNums.Remove(num);
                                matrix[x, y].num = num;
                            }
                            else
                                Debug.LogError("num is not enough");
                        }
                    }
                }
                #endregion
            }
            card_num_matrixs.Add(matrix);
        }
        ballNums = ConvertDoubleNestListToOneList(ballUseNumPerColumns);
        #region 填充球
        if (ballNums.Count < MaxBallCount)
        {
            int needAddBallCount = MaxBallCount - ballNums.Count;
            List<int> unUseNums = ConvertDoubleNestListToOneList(unUseNumPerColumns);
            for(int i = 0; i < needAddBallCount; i++)
            {
                int ball_new_num = unUseNums[Random.Range(0, unUseNums.Count)];
                unUseNums.Remove(ball_new_num);
                ballNums.Add(ball_new_num);
            }
        }
        #endregion
        return card_num_matrixs;
    }
    public int CheckBingoCount(Card_Bingo_Calculate[,] matrix)
    {
        int xLength = matrix.GetLength(0);
        int cardSize = Columns_Count;
        int bingoCount = 0;
        int normalBingoNeedNum = cardSize;
        int cornerBingoNeedNum = 4;
        for (int x = 1; x < xLength - 1; x++)
        {
            Card_Bingo_Calculate horizontalCalculate = matrix[0, x];
            Card_Bingo_Calculate verticalCalculate = matrix[x, 0];
            if (horizontalCalculate.horizontal == normalBingoNeedNum)
                bingoCount++;
            if (verticalCalculate.vertical == normalBingoNeedNum)
                bingoCount++;
        }
        Card_Bingo_Calculate zeroCalculate = matrix[0, 0];
        if (zeroCalculate.horizontal == normalBingoNeedNum)
            bingoCount++;
        if (zeroCalculate.vertical == normalBingoNeedNum)
            bingoCount++;
        if (zeroCalculate.slopeA == normalBingoNeedNum)
            bingoCount++;
        if (zeroCalculate.slopeB == normalBingoNeedNum)
            bingoCount++;
        if (zeroCalculate.corner == cornerBingoNeedNum)
            bingoCount++;
        Card_Bingo_Calculate left_downCalculate = matrix[0, cardSize - 1];
        if (left_downCalculate.horizontal == normalBingoNeedNum)
            bingoCount++;
        if (left_downCalculate.slopeB == normalBingoNeedNum)
            bingoCount++;
        Card_Bingo_Calculate right_upCalculate = matrix[cardSize - 1, 0];
        if (right_upCalculate.vertical == normalBingoNeedNum)
            bingoCount++;
        return bingoCount;
    }
    public int GetDoubleNestListCount<T>(List<List<T>> nestList)
    {
        int count = 0;
        foreach (var list in nestList)
            count += list.Count;
        return count;
    }
    public List<List<T>> CloneDoubleNestList<T>(List<List<T>> oldNestList)
    {
        if (oldNestList == null)
            return null;
        List<List<T>> cloneList = new List<List<T>>();
        int count = oldNestList.Count;
        for(int i = 0; i < count; i++)
        {
            cloneList.Add(new List<T>());
            List<T> childList = oldNestList[i];
            int childCount =childList.Count;
            if(childCount>0)
                for (int j = 0; j < childCount; j++)
                {
                    cloneList[i].Add(childList[j]);
                }
        }
        return cloneList;
    }
    public List<T> ConvertDoubleNestListToOneList<T>(List<List<T>> doubleNestList)
    {
        if (doubleNestList == null)
            return null;
        List<T> result = new List<T>();
        int count = doubleNestList.Count;
        for(int i = 0; i < count; i++)
        {
            List<T> childList = doubleNestList[i];
            int childCount = childList.Count;
            for(int j = 0; j < childCount; j++)
            {
                result.Add(childList[j]);
            }
        }
        return result;
    }
    private void ChangePage(int page)
    {
        int dataCount = all_panel_cardItem_Datas.Count;
        if (page > dataCount / 4 + 1)
            Debug.LogError("page is more than max page, dataCount: " + dataCount);
        else
        {
            int startIndex = 4 * (page - 1);
            int endIndex = Mathf.Min(dataCount - 1, 4 * page - 1);
            int numTextSize;
            if (dataCount == 1)
                numTextSize = CardNumTextSize[0];
            else if (dataCount == 2)
                numTextSize = CardNumTextSize[1];
            else
                numTextSize = CardNumTextSize[2];
            for(int i = startIndex; i <= endIndex; i++)
            {
                int panelIndex = i - startIndex;
                int dataPanelIndex = i;
                int xLength = all_panel_cardItems[panelIndex].GetLength(0);
                int yLength = all_panel_cardItems[panelIndex].GetLength(1);
                for(int x = 0; x < xLength; x++)
                {
                    for(int y = 0; y < yLength; y++)
                    {
                        Card_ItemData itemData = all_panel_cardItem_Datas[i][x, y];
                        all_panel_cardItems[panelIndex][x, y].Init(x, y, dataPanelIndex, itemData.num, itemData.hasOpen, itemData.reward, numTextSize, itemData.bingoReward);
                    }
                }
            }
        }
    }
    private int extraRewardTime = 0;
    private List<Config_CardExtraReward> config_CardExtraRewards = null;
    private ExtraReward RandomNextCardExtraReward(out int rewardNum)
    {
        if (config_CardExtraRewards == null)
            config_CardExtraRewards = ConfigManager.GetRandomExtraReward();
        int goldNum = SaveManager.SaveData.gold;
        int cashNum = SaveManager.SaveData.cash;
        int configCount = config_CardExtraRewards.Count;
        #region 阈值限制
        for (int i = 0; i < configCount; i++)
        {
            Reward limitType = config_CardExtraRewards[i].appearLimitType;
            switch (limitType)
            {
                case Reward.Gold:
                    if (goldNum >= config_CardExtraRewards[i].appearLimitMax)
                        config_CardExtraRewards[i] = new Config_CardExtraReward()
                        {
                            rewardType = config_CardExtraRewards[i].rewardType,
                            weight = config_CardExtraRewards[i].weight,
                            numRange = config_CardExtraRewards[i].numRange,
                            mustBeInThreeTimes = -1,
                            appearLimitType = config_CardExtraRewards[i].appearLimitType,
                            appearLimitMax = config_CardExtraRewards[i].appearLimitMax
                        };
                    break;
                case Reward.Cash:
                    if (cashNum >= config_CardExtraRewards[i].appearLimitMax)
                        config_CardExtraRewards[i] = new Config_CardExtraReward()
                        {
                            rewardType = config_CardExtraRewards[i].rewardType,
                            weight = config_CardExtraRewards[i].weight,
                            numRange = config_CardExtraRewards[i].numRange,
                            mustBeInThreeTimes = -1,
                            appearLimitType = config_CardExtraRewards[i].appearLimitType,
                            appearLimitMax = config_CardExtraRewards[i].appearLimitMax
                        };
                    break;
            }
        }
        #endregion
        if (extraRewardTime < 3)
        {
            extraRewardTime++;
            int total = 0;
            for(int i = 0; i < configCount; i++)
            {
                if (config_CardExtraRewards[i].mustBeInThreeTimes > 0)
                    total += config_CardExtraRewards[i].weight;
            }
            int result = Random.Range(0, total);
            int max = 0;
            for(int i = 0; i < configCount; i++)
            {
                if (config_CardExtraRewards[i].mustBeInThreeTimes > 0)
                {
                    max += config_CardExtraRewards[i].weight;
                    if (result < max)
                    {
                        rewardNum = config_CardExtraRewards[i].numRange.RandomIncludeMax();
                        config_CardExtraRewards[i] = new Config_CardExtraReward()
                        {
                            rewardType = config_CardExtraRewards[i].rewardType,
                            weight = config_CardExtraRewards[i].weight,
                            numRange = config_CardExtraRewards[i].numRange,
                            mustBeInThreeTimes = config_CardExtraRewards[i].mustBeInThreeTimes - 1,
                            appearLimitType = config_CardExtraRewards[i].appearLimitType,
                            appearLimitMax = config_CardExtraRewards[i].appearLimitMax
                        };
                        return config_CardExtraRewards[i].rewardType;
                    }
                }
            }
            Debug.LogError("error extra reward result");
        }
        else
        {
            extraRewardTime++;
            int total = 0;
            for (int i = 0; i < configCount; i++)
            {
                if (config_CardExtraRewards[i].mustBeInThreeTimes > -1)
                    total += config_CardExtraRewards[i].weight;
            }
            int result = Random.Range(0, total);
            int max = 0;
            for (int i = 0; i < configCount; i++)
            {
                if (config_CardExtraRewards[i].mustBeInThreeTimes > -1)
                {
                    max += config_CardExtraRewards[i].weight;
                    if (result < max)
                    {
                        rewardNum = config_CardExtraRewards[i].numRange.RandomIncludeMax();
                        return config_CardExtraRewards[i].rewardType;
                    }
                }
            }
            Debug.LogError("error extra reward result");
        }
        Debug.LogError("no extra reward random");
        rewardNum = 0;
        return ExtraReward.Empty;
    }
    private void ResetExtraReward()
    {
        extraRewardTime = 0;
        clickBingoCardTime = 0;
        config_CardExtraRewards = null;
        extra_reward_progress_fillImage.fillAmount = 0;
        foreach (var image in extra_reward_flyImages)
            image.gameObject.SetActive(false);
    }
    int clickBingoCardTime = 0;
    public Image extra_reward_progress_fillImage;
    public Image extra_reward_iconImage;
    public Image single_extra_reward_flyImage;
    private readonly List<Image> extra_reward_flyImages = new List<Image>();
    private IEnumerator AutoIncreaseExtraRewardProgress(int panelNum)
    {
        int currentProgress = 0;
        float maxProgress;
        if (panelNum == 1)
            maxProgress = 2;
        else if (panelNum == 2)
            maxProgress = 3;
        else
            maxProgress = 4;
        extra_reward_progress_fillImage.fillAmount = currentProgress / maxProgress;
        ExtraReward extraRewardType = RandomNextCardExtraReward(out int rewardCardNum);
        extra_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, extraRewardType.ToString());
        extra_reward_iconImage.gameObject.SetActive(false);
        extra_reward_iconImage.gameObject.SetActive(true);
        while (true)
        {
            if (clickBingoCardTime > 0 && !isPause)
            {
                clickBingoCardTime--;
                currentProgress += 1;
                float startProgress = extra_reward_progress_fillImage.fillAmount;
                float endProgress = currentProgress / maxProgress;
                float timer = 0;
                while (timer < 1)
                {
                    timer += Time.deltaTime*2;
                    yield return null;
                    extra_reward_progress_fillImage.fillAmount = Mathf.Lerp(startProgress, endProgress, timer);
                }
                if (currentProgress == maxProgress)
                {
                    AudioManager.PlayOneShot(AudioPlayArea.ExtraRewardFill);
                    currentProgress = 0;
                    if (!SaveManager.SaveData.hasFirstGetExtraReward[(int)extraRewardType])
                    {
                        isPause = true;
                        UIManager.ShowPanel(PopPanel.FirstGetExtraRewardPanel, (int)extraRewardType);
                    }
                    extra_reward_iconImage.gameObject.SetActive(false);
                    extra_reward_iconImage.gameObject.SetActive(true);
                    List<List<Vector3>> allPanelTargetCardWorldPos = new List<List<Vector3>>();
                    List<List<Vector3Int>> allPanelTargetCardPos_x_y = new List<List<Vector3Int>>();
                    for (int i = 0; i < panelNum; i++)
                    {
                        List<Vector2Int> panelTargetCardPos = RandomExtraRewardPos(extraRewardType, i, rewardCardNum);
                        List<Vector3> convertPos = ConvertListType(panelTargetCardPos, (pos) => { return all_panel_cardItems[i][pos.x, pos.y].transform.position; });
                        int panelIndex = i;
                        List<Vector3Int> convertPos_x_y = ConvertListType(panelTargetCardPos, (pos) => { return new Vector3Int(pos.x, pos.y, panelIndex); });
                        allPanelTargetCardPos_x_y.Add(convertPos_x_y);
                        allPanelTargetCardWorldPos.Add(convertPos);
                    }
                    yield return new WaitForSeconds(0.5f);
                    Vector3 startPos = extra_reward_iconImage.transform.position;
                    List<Vector3> targetPos = ConvertDoubleNestListToOneList(allPanelTargetCardWorldPos);
                    int flyCount = targetPos.Count;
                    Vector2 flySize = new Vector2(single_card.Rect.sizeDelta.x, single_card.Rect.sizeDelta.x)*1.2f;
                    for (int flyIndex = 0; flyIndex < flyCount; flyIndex++)
                    {
                        if(flyIndex> extra_reward_flyImages.Count - 1)
                        {
                            Image newFlyImage = Instantiate(single_extra_reward_flyImage.gameObject, single_extra_reward_flyImage.transform.parent).GetComponent<Image>();
                            extra_reward_flyImages.Add(newFlyImage);
                        }
                        extra_reward_flyImages[flyIndex].sprite = extra_reward_iconImage.sprite;
                        extra_reward_flyImages[flyIndex].transform.position = startPos;
                        extra_reward_flyImages[flyIndex].GetComponent<RectTransform>().sizeDelta = flySize;
                        extra_reward_flyImages[flyIndex].gameObject.SetActive(true);
                    }
                    float flyTimer = 0;
                    float speed = 1.5f;
                    while (flyTimer < 1)
                    {
                        if (!isPause)
                        {
                            if (flyTimer > 0.9f)
                            {
                                speed -= 0.1f;
                                speed = Mathf.Clamp(speed, 0.3f, 3);
                            }
                            flyTimer += Time.deltaTime * speed;
                            for (int i = 0; i < flyCount; i++)
                            {
                                extra_reward_flyImages[i].transform.position = Vector3.Lerp(startPos, targetPos[i], flyTimer);
                            }
                        }
                        yield return null;
                    }
                    List<Vector3Int> targetPos_x_y = ConvertDoubleNestListToOneList(allPanelTargetCardPos_x_y);
                   for (int flyIndex = 0; flyIndex < flyCount; flyIndex++)
                    {
                        extra_reward_flyImages[flyIndex].gameObject.SetActive(false);
                        Vector3Int target_x_y = targetPos_x_y[flyIndex];
                        switch (extraRewardType)
                        {
                            case ExtraReward.ExtraGold:
                                all_panel_cardItems[target_x_y.z][target_x_y.x, target_x_y.y].SetReward(Reward.Gold);
                                break;
                            case ExtraReward.TribleReward:
                                all_panel_cardItems[target_x_y.z][target_x_y.x, target_x_y.y].MultipleReward();
                                break;
                            case ExtraReward.Star:
                                all_panel_cardItems[target_x_y.z][target_x_y.x, target_x_y.y].ForceBingo();
                                break;
                        }
                    }
                    switch (extraRewardType)
                    {
                        case ExtraReward.ExtraGold:
                            AudioManager.PlayOneShot(AudioPlayArea.ExtraGoldReach);
                            break;
                        case ExtraReward.TribleReward:
                            AudioManager.PlayOneShot(AudioPlayArea.TripleReach);
                            break;
                        case ExtraReward.Star:
                            AudioManager.PlayOneShot(AudioPlayArea.StarReach);
                            break;
                    }
                    yield return new WaitForSeconds(0.5f);
                    extraRewardType = RandomNextCardExtraReward(out rewardCardNum);
                    extra_reward_iconImage.sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, extraRewardType.ToString());
                    extra_reward_iconImage.gameObject.SetActive(false);
                    extra_reward_iconImage.gameObject.SetActive(true);
                    while (timer > 0)
                    {
                        timer -= Time.deltaTime*2;
                        yield return null;
                        extra_reward_progress_fillImage.fillAmount = Mathf.Lerp(0, 1, timer);
                    }
                }
            }
            yield return null;
        }
    }
    private List<Vector2Int> RandomExtraRewardPos(ExtraReward extraReward, int panelIndex, int needNum)
    {
        List<List<int>> targetList;
        switch (extraReward)
        {
            case ExtraReward.ExtraGold:
                targetList = CloneDoubleNestList(noRewardCardPos[panelIndex]);
                break;
            case ExtraReward.TribleReward:
                targetList = CloneDoubleNestList(hasRewardCardPos[panelIndex]);
                break;
            case ExtraReward.Star:
                targetList = CloneDoubleNestList(notBingoCardPos[panelIndex]);
                break;
            default:
                return null;
        }
        int availableCount = GetDoubleNestListCount(targetList);
        needNum = Mathf.Min(needNum, availableCount);
        List<Vector2Int> cardPos = new List<Vector2Int>();
        if (all_panel_bingoCount[panelIndex] >= 3)
            return cardPos;
        while (needNum > 0)
        {
            int x = Random.Range(0, targetList.Count);
            if (targetList[x].Count > 0)
            {
                List<int> targetListYList = targetList[x];
                int y = targetListYList[Random.Range(0, targetListYList.Count)];
                switch (extraReward)
                {
                    case ExtraReward.ExtraGold:
                        noRewardCardPos[panelIndex][x].Remove(y);
                        hasRewardCardPos[panelIndex][x].Add(y);
                        break;
                    case ExtraReward.TribleReward:
                        break;
                    case ExtraReward.Star:
                        notBingoCardPos[panelIndex][x].Remove(y);
                        break;
                }
                targetListYList.Remove(y);
                cardPos.Add(new Vector2Int(x, y));
                needNum--;
            }
        }
        return cardPos;
    }
    private List<ToType> ConvertListType<FromType,ToType>(List<FromType> originList,System.Func<FromType,ToType> convertFunc)
    {
        List<ToType> resultList = new List<ToType>();
        if (originList == null)
            return null;
        if (originList.Count <= 0)
            return resultList;
        int originListCount = originList.Count;
        for(int i = 0; i < originListCount; i++)
        {
            resultList.Add(convertFunc(originList[i]));
        }
        return resultList;
    }
    int totalPigCashThisTime = 0;
    public void SendPanelShowArgs(params int[] args)
    {
        isPause = true;
        totalPigCashThisTime = 0;
        hasGetMoreBall = false;
        gameState = GameState.Start;
        allGetRewards.Clear();
        UpdateKey();
        AudioManager.PlayOneShot(AudioPlayArea.ReadyGo);
        generateCor = StartCoroutine(GenerateCards(args[0]));
    }
    public void OnShowAniamtionEnd()
    {
        isPause = false;
        gameState = GameState.Playing;
    }
    public void SendArgs(int sourcePanelIndex, params int[] args)
    {
        switch (sourcePanelIndex)
        {
            case (int)PopPanel.BackToMainPanel:
                if (args[0] == 1)
                {
                    GameManager.Instance.SendAdjustGameEndEvent(false, all_panel_cardItem_Datas.Count);
                    UIManager.HidePanel(gameObject);
                    UIManager.ShowPanel(BasePanel.MainPanel);
                    UIManager.SendMessageToPanel(BasePanel.GamePanel, BasePanel.Menu, 0);
                    generateCor = null;
                }
                else if (args[0] == 0)
                {
                    isPause = false;
                }
                break;
            case (int)BasePanel.Menu:
                if (args[0] == 1)
                    isPause = true;
                else if (gameState == GameState.Playing)
                    isPause = false;
                break;
            case (int)PopPanel.GameOverPanel:
                if (args[0] == 1)
                {
                    UIManager.HidePanel(gameObject);
                    UIManager.ShowPanel(BasePanel.MainPanel);
                    UIManager.SendMessageToPanel(BasePanel.GamePanel, BasePanel.Menu, 0);
                    generateCor = null;
                }
                else if (args[0] == 0)
                {
                    animator.SetBool("GameOver", false);
                    GameManager.Instance.ChangeCardNum(-all_panel_cardItem_Datas.Count, BasePanel.GamePanel);
                    SendPanelShowArgs(all_panel_cardItem_Datas.Count);
                }
                break;
            case (int)PopPanel.GuidePanel:
                isPause = false;
                if (args[0] == (int)GuideType.GameGuide_ClickCard && args[1] == 1)
                    all_panel_cardItems[0][0, 0].ForceBingo();
                break;
            case (int)PopPanel.SlotsPanel:
                int extraBallCount = args[0];
                if (extraBallCount > 0)
                {
                    int usefulBallCount = ConfigManager.GetMoreBallUsefulCount(extraBallCount);
                    StopCoroutine(GenerateBallCoroutine);
                    GenerateBallCoroutine = StartCoroutine(GenerateNext(false, RandomExtraBallNums(extraBallCount, usefulBallCount), true));
                    animator.SetBool("Continue", true);
                    animator.SetBool("GameOver", false);
                }
                else
                    OnGameOverAnimationEnd();
                break;
            case (int)PopPanel.WheelPanel:
                break;
            case (int)PopPanel.GetMoreKeyPanel:
                if (SaveManager.SaveData.key >= 3 && !GetWillShowWheelPanel())
                {
                    UIManager.ShowPanel(PopPanel.DrawBoxPanel, (int)BasePanel.GamePanel);
                }
                else if (gameState == GameState.Playing)
                    isPause = false;
                break;
            case (int)PopPanel.HelpPanel:
            case (int)PopPanel.DrawBoxPanel:
                if (gameState == GameState.Playing)
                    isPause = false;
                break;
            case (int)PopPanel.MoneyBoxPanel:
                FinalGameOver();
                break;
            case (int)PopPanel.EnterCashoutTaskPanel:
                if (gameState == GameState.Playing)
                    isPause = false;
                break;
            case (int)BasePanel.System:
                UpdateKey();
                break;
        }
    }
    public void OnIdleEnd()
    {
        animator.SetBool("Continue", false);
    }
    private List<int> RandomExtraBallNums(int count,int usefulCount)
    {
        List<int> panelIndexes = new List<int>();
        int panelCount = all_panel_bingoCount.Count;
        for(int i = 0; i < panelCount; i++)
        {
            if (all_panel_bingoCount[i] < 3)
                panelIndexes.Add(i);
        }
        int canSelectPanelCount = panelIndexes.Count;
        List<int> unUsefulNum = new List<int>();
        for(int i = 0; i < Columns_Count; i++)
        {
            for (int j = 0; j < NUM_PER_COLUMNS; j++)
                unUsefulNum.Add(i * NUM_PER_COLUMNS + j + 1);
        }
        int hasShowBallCount = hasShowBallNums.Count;
        for(int i = 0; i < hasShowBallCount; i++)
            unUsefulNum.Remove(hasShowBallNums[i]);

        List<int> usefulNum = new List<int>();
        for(int i = 0; i < canSelectPanelCount; i++)
        {
            Card_ItemData[,] datas = all_panel_cardItem_Datas[panelIndexes[i]];
            for(int x = 0; x < Columns_Count; x++)
            {
                for(int y = 0; y < Columns_Count; y++)
                {
                    Card_ItemData card = datas[x, y];
                    if (!card.hasOpen)
                    {
                        int num = card.num;
                        if (!hasShowBallNums.Contains(num) && !usefulNum.Contains(num))
                        {
                            usefulNum.Add(num);
                            unUsefulNum.Remove(num);
                        }
                    }
                }
            }
        }

        List<int> result = new List<int>();
        for(int i = 0; i < count; i++)
        {
            if (i < usefulCount && usefulNum.Count > 0)
            {
                int num = usefulNum[Random.Range(0, usefulNum.Count)];
                result.Add(num);
                usefulNum.Remove(num);
            }
            else
            {
                int num = unUsefulNum[Random.Range(0, unUsefulNum.Count)];
                result.Add(num);
                unUsefulNum.Remove(num);
            }
        }
        return result;
    }
    public void TriggerPanelHideEvent()
    {
        gameState = GameState.Unknow;
    }
    public void Pause()
    {
        isPause = true;
    }
    [Space(15)]
    public List<Image> keysImage = new List<Image>();
    public void UpdateKey()
    {
        int currentKey = SaveManager.SaveData.key - 1;
        for(int i = 0; i < 3; i++)
        {
            if (i <= currentKey)
                keysImage[i].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, "key_on");
            else
                keysImage[i].sprite = SpriteManager.GetSprite(SpriteAtlas_Name.Game, "key_off");
        }
    }
    public Button helpButton;
    private void OnHelpButtonClick()
    {
        if (GetWillShowWheelPanel())
            return;
        isPause = true;
        UIManager.ShowPanel(PopPanel.HelpPanel,(int)BasePanel.GamePanel);
    }
    public Button keyButton;
    private void OnKeyButtonClick()
    {
        if (GetWillShowWheelPanel())
            return;
        isPause = true;
        UIManager.ShowPanel(PopPanel.GetMoreKeyPanel);
    }
}
public struct Card_ItemData
{
    public int num;
    public Reward reward;
    public Reward bingoReward;
    public bool hasOpen;
    public Card_Bingo_Calculate _Calculate;
}
public struct Card_Bingo_Calculate
{
    public int num;
    public int horizontal;
    public int vertical;
    public int slopeA;
    public int slopeB;
    public int corner;
    public Reward reward;
}
public enum BingoFillType
{
    Horizontal,
    Vertical,
    SlopeA,
    SlopeB,
    Corner,
    Null
}
public enum GameState
{
    Unknow,
    Start,
    Playing,
    GameOver
}

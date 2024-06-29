using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField]
    int size;
    int turn = 0;

    bool Xturn = true;
    bool playable = true;

    enum Slot { Empty, X, O };

    Slot[,] playslot;
    GameObject[] playGO;
    history[] MoveHistory;

    [Header("GameObject & UI")]
    public GameObject XPrefab;
    public GameObject OPrefab;
    public GameObject xText, oText;
    public GameObject ResultUI;
    public GameObject[] Table;
    public Text ResultText;

    void Start()
    {
        startSetting(size);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && playable)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                position pos;
                pos = hit.transform.GetComponent<position>();
                place(pos.x, pos.y, hit.transform, true);
            }
        }
    }

    public void startSetting(int s)
    {
        size = s;
        playGO = new GameObject[s * s];
        MoveHistory = new history[s * s];
        playslot = new Slot[s, s];

        TurnUIupdate();
    }

    void place(int x, int y, Transform t, bool c)
    {
        if (playslot[x, y] == Slot.Empty)
        {
            playslot[x, y] = (Xturn) ? Slot.X : Slot.O;

            GameObject temp = Instantiate((Xturn) ? XPrefab : OPrefab, t.position, Quaternion.identity);
            playGO[turn] = temp;
            MoveHistory[turn] = new history(x, y, t);

            checkwin((Xturn) ? Slot.X : Slot.O);
            Xturn = !Xturn;
            turn++;

            if (c) clearHistory(turn);
            TurnUIupdate();
        }
    }

    public void Restart()
    {
        for (int i = 0; i < playGO.Length; i++)
        {
            Destroy(playGO[i]);
        }
        ResetSlot();

        turn = 0;
        clearHistory(turn);

        ResultUIUpdate(false, 0);
        playable = true;
    }

    void ResetSlot()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                playslot[i, j] = Slot.Empty;
            }
        }
    }
    void clearHistory(int t)
    {
        for (int i = t; i < MoveHistory.Length; i++)
        {
            if (MoveHistory[i] != null)
                MoveHistory[i].clearData();
        }
    }

    void checkwin(Slot s)
    {
        for (int i = 0; i < size; i++)
        {
            int c1 = 0;
            int c2 = 0;
            for (int j = 0; j < size; j++)
            {
                if (playslot[i, j] == s) c1++;
                if (playslot[j, i] == s) c2++;
            }
            if (c1 == size || c2 == size)
            {
                PlayerWin(s);
                break;
            }
        }

        int c3 = 0;
        for (int i = 0; i < size; i++)
        {
            if (playslot[i, i] == s) c3++;
        }
        if (c3 == size) PlayerWin(s);

        int c4 = 0;
        for (int i = 0; i < size; i++)
        {
            if (playslot[i, (size - 1) - i] == s) c4++;
        }
        if (c4 == size) PlayerWin(s);

        if (turn + 1 == size * size && playable)
        {
            ResultUIUpdate(true, 0);
            playable = false;
        }
    }
    void PlayerWin(Slot s)
    {
        ResultUIUpdate(true, (s == Slot.X) ? 1 : 2);
        playable = false;
    }

    public void undoMove()
    {
        if (turn > 0 && playable)
        {
            turn--;
            int x = MoveHistory[turn].Xpos;
            int y = MoveHistory[turn].Ypos;
            playslot[x, y] = Slot.Empty;

            Destroy(playGO[turn]);

            Xturn = !Xturn;
            TurnUIupdate();
        }
    }
    public void redoMove()
    {
        if (MoveHistory[turn] != null && MoveHistory[turn].hasData && playable)
        {
            history temp = MoveHistory[turn];
            place(temp.Xpos, temp.Ypos, temp.transform, false);
        }
    }

    void TurnUIupdate()
    {
        xText.SetActive(false);
        oText.SetActive(false);
        ((Xturn) ? xText : oText).SetActive(true);
    }
    void ResultUIUpdate(bool active, int p)
    {
        if (active)
        {
            string t;
            switch (p)
            {
                case 1: t = "X Win!"; break;
                case 2: t = "O Win!"; break;
                default: t = "Draw"; break;
            }
            ResultText.text = t;
        }
        ResultUI.SetActive(active);
    }

    public void ChangeTable(int t)
    {
        Restart();
        for (int i = 0; i < Table.Length; i++)
        {
            Table[i].SetActive(false);
        }
        Table[t].SetActive(true);
    }
}

public class history
{
    public int Xpos;
    public int Ypos;
    public Transform transform;
    public bool hasData = false;
    public history(int x, int y, Transform t)
    {
        this.Xpos = x;
        this.Ypos = y;
        this.transform = t;
        hasData = true;
    }
    public void clearData()
    {
        hasData = false;
    }
}

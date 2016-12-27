﻿#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DialogBoxTrade : DialogBox
{
    public Text TraderNameText;
    public Text PlayerCurrencyBalanceText;
    public Text TraderCurrencyBalanceText;
    public Text TradeCurrencyBalanceText;
    public Transform TradeItemListPanel;

    public GameObject TradeItemPrefab;
    public GameObject TradeRequestPrefab;

    public Action TradeCompleted;
    public Action TradeCancelled;

    public Button AcceptButton;

    private Trade trade;

    public void SetupTrade(Trade trade)
    {
        this.trade = trade;

        ClearInterface();
        BuildInterface();
    }

    public void DoTradingTestWithMockTraders()
    {
        Currency currency = PrototypeManager.Currency.Get("Quill Corp Bucks");

        Currency playerCurrency = currency.Clone();
        playerCurrency.Balance = 1000f;

        Trader mockPlayer = new Trader
        {
            Currency = playerCurrency,
            Name = "Player",
            SaleMarginMultiplier = 1f,
            Stock = new List<Inventory>
            {
                new Inventory("Steel Plate", 10) { BasePrice = 3f },
                new Inventory("Raw Iron", 90) { BasePrice = 0.2f },
            }
        };

        Currency traderCurrency = currency.Clone();
        traderCurrency.Balance = 1000f;

        Trader mockTrader = new Trader
        {
            Currency = traderCurrency,
            Name = "Trader",
            SaleMarginMultiplier = 1.23f,
            Stock = new List<Inventory>
            {
                new Inventory("Steel Plate", 40) { BasePrice = 3f },
                new Inventory("Steel Plate", 40) { BasePrice = 3f },
                new Inventory("Oxygen Bottle", 10) { BasePrice = 50f },
            }
        };
        SetupTrade(new Trade(mockPlayer, mockTrader));
    }

    public void CancelTrade()
    {
        trade = null;
        ClearInterface();
        CloseDialog();
        if (TradeCompleted != null)
        {
            TradeCancelled();
        }
    }

    public void AcceptTrade()
    {
        if (trade.IsValid())
        {
            ClearInterface();
            if (TradeCompleted != null)
            {
                TradeCompleted();
            }
        }
    }
    
    public void FinalizeRequests()
    {
        for (int i = 0; i < TradeItemListPanel.transform.childCount; i++)
        {
            GameObject requestObject = TradeItemListPanel.transform.GetChild(i).gameObject;
            Toggle[] toggles = requestObject.GetComponentsInChildren<Toggle>();
            RequestUrgency urgency = (RequestUrgency)Enum.Parse(typeof(RequestUrgency), toggles.Where(x => x.isOn).First().name);
            trade.Trader.Prototype.Requests.Add(new TradeItemRequest(requestObject.GetComponentInChildren<Text>().text, urgency));
        }

        FinalizeTrading();
    }   

    public void FinalizeTrading ()
    {
        trade = null;
        ClearInterface();
        CloseDialog();
    }

    // TODO -- maybe add trader requests in the future?

    public void PrepareUIForRequests ()
    {
        BuildRequestHeader();

        foreach (TraderPotentialRequest potentialRequest in trade.Trader.Prototype.PotentialRequests)
        {
            if (!string.IsNullOrEmpty(potentialRequest.Request))
            {
                TradeItemRequest request = new TradeItemRequest(potentialRequest.Request, RequestUrgency.NONE);
                AddRequestPanel(request);
            }
            else if (!string.IsNullOrEmpty(potentialRequest.Category))
            {
                List<InventoryCommon> common = GetInventoryCommonWithCategory(potentialRequest.Category);

                foreach (InventoryCommon inventoryCommon in common)
                {
                    TradeItemRequest request = new TradeItemRequest(inventoryCommon.Type, RequestUrgency.NONE);
                    AddRequestPanel(request);
                }
            }
        }
    }

    private List<InventoryCommon> GetInventoryCommonWithCategory(string category)
    {
        return PrototypeManager.Inventory.Values.Where(i => i.category == category).ToList();
    }

    private void BuildRequestHeader()
    {
        PlayerCurrencyBalanceText.text = "";
        TraderCurrencyBalanceText.text = "";
        TraderCurrencyBalanceText.alignment = TextAnchor.MiddleRight;
        TradeCurrencyBalanceText.text = "None Low";
        TraderNameText.alignment = TextAnchor.MiddleCenter;
        TraderNameText.text = "Medium High Urgent";
    }

    private void AddRequestPanel(TradeItemRequest request)
    {
        GameObject go = (GameObject)Instantiate(TradeRequestPrefab, TradeItemListPanel);
        go.transform.GetChild(0).GetComponent<Text>().text = request.ItemType;
    }

    private void ClearInterface()
    {
        List<Transform> childrens = TradeItemListPanel.Cast<Transform>().ToList();
        foreach (Transform child in childrens)
        {
            Destroy(child.gameObject);
        }
    }

    private void BuildInterface()
    {
        TraderNameText.text = trade.Trader.Name;
        BuildInterfaceHeader();

        foreach (TradeItem tradeItem in trade.TradeItems)
        {
            GameObject go = (GameObject)Instantiate(Resources.Load("UI/Components/TradeItemPrefab"), TradeItemListPanel);

            DialogBoxTradeItem tradeItemBehaviour = go.GetComponent<DialogBoxTradeItem>();
            tradeItemBehaviour.OnTradeAmountChangedEvent += item => BuildInterfaceHeader();
            tradeItemBehaviour.SetupTradeItem(tradeItem);
        }
    }

    private void BuildInterfaceHeader()
    {
        float tradeAmount = trade.TradeCurrencyBalanceForPlayer;
        PlayerCurrencyBalanceText.text = string.Format(
            "{0:N2} {1}", 
            trade.Player.Currency.Balance + trade.TradeCurrencyBalanceForPlayer, 
            trade.Player.Currency.ShortName);
        TraderCurrencyBalanceText.text = string.Format(
            "{0:N2} {1}", 
            trade.Trader.Currency.Balance - trade.TradeCurrencyBalanceForPlayer, 
            trade.Trader.Currency.ShortName);
        TradeCurrencyBalanceText.text = tradeAmount.ToString("N2");

        AcceptButton.interactable = trade.IsValid();
    }
}
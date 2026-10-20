using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ValleyInvest;

public sealed class ModEntry : Mod
{
    private const string SaveKey = "investment-account";
    private InvestmentAccount? account;
    private ModConfig config = new();

    public override void Entry(IModHelper helper)
    {
        config = helper.ReadConfig<ModConfig>();
        if (config.DailyInterestRate < 0 || config.DailyInterestRate > 1)
        {
            Monitor.Log("DailyInterestRate doit être compris entre 0 et 1. Taux utilisé : 0.01.", LogLevel.Warn);
            config.DailyInterestRate = 0.01m;
        }

        helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        helper.Events.GameLoop.DayEnding += OnDayEnding;
        helper.Events.GameLoop.Saving += (_, _) =>
        {
            if (Context.IsMainPlayer && account is not null)
                helper.Data.WriteSaveData(SaveKey, account);
        };
        helper.Events.GameLoop.ReturnedToTitle += (_, _) => account = null;
        helper.Events.Input.ButtonPressed += OnButtonPressed;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        account = null;
        if (!Context.IsMainPlayer)
            return;
        try
        {
            var saved = Helper.Data.ReadSaveData<InvestmentAccount>(SaveKey) ?? new InvestmentAccount();
            if (!saved.IsValid)
                throw new InvalidDataException("Le compte contient des valeurs invalides.");
            account = saved;
        }
        catch (Exception ex)
        {
            Monitor.Log($"Compte indisponible ; données conservées sans modification. {ex}", LogLevel.Error);
        }
    }

    private void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        if (Context.IsMainPlayer && !Game1.IsMultiplayer)
            account?.ApplyInterest((int)Game1.stats.DaysPlayed, config.DailyInterestRate);
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsWorldReady || !Context.IsPlayerFree || !e.Button.IsActionButton()
            || Game1.currentLocation.Name != "ManorHouse")
            return;

        var tile = e.Cursor.GrabTile;
        var npc = Game1.currentLocation.isCharacterAtTile(tile);
        if (npc?.Name != "Lewis"
            || Microsoft.Xna.Framework.Vector2.Distance(Game1.player.Tile, tile) > 1.5f)
            return;

        Helper.Input.Suppress(e.Button);
        if (Game1.IsMultiplayer)
        {
            Say("La première version de Valley Invest est disponible en solo uniquement.");
            return;
        }
        if (account is null)
        {
            Say("Le compte est indisponible. Consultez le journal SMAPI.");
            return;
        }
        Game1.currentLocation.createQuestionDialogue("Valley Invest — Que souhaitez-vous faire ?",
            new[]
            {
                new Response("deposit", "Déposer de l’or"),
                new Response("balance", "Consulter mon compte"),
                new Response("withdraw", "Retirer de l’or"),
                new Response("talk", "Parler à Lewis"),
                new Response("cancel", "Fermer")
            },
            (_, answer) =>
            {
                switch (answer)
                {
                    case "deposit": SelectAmount(true); break;
                    case "withdraw": SelectAmount(false); break;
                    case "balance": ShowBalance(); break;
                    case "talk": npc.checkAction(Game1.player, Game1.currentLocation); break;
                }
            });
    }

    private void ShowBalance()
    {
        if (account is null) return;
        Say($"Solde placé : {account.Balance:N2} po^"
            + $"Intérêts cumulés : {account.TotalInterest:N2} po^"
            + $"Derniers intérêts : {account.LastInterest:N2} po^"
            + $"Taux quotidien : {config.DailyInterestRate:P2}^"
            + "Les intérêts sont réinvestis chaque nuit.");
    }

    private void SelectAmount(bool deposit)
    {
        if (account is null) return;
        int maximum = deposit ? account.DepositLimit(Game1.player.Money) : account.WithdrawalLimit(Game1.player.Money);
        if (maximum < 1)
        {
            Say(deposit ? "Dépôt impossible : portefeuille vide ou compte au plafond."
                : "Retrait impossible : solde inférieur à 1 po ou portefeuille au plafond.");
            return;
        }
        Game1.activeClickableMenu = new NumberSelectionMenu(
            deposit ? "Montant à déposer (po)" : "Montant à retirer (po)",
            (amount, _, _) => Transfer(deposit, amount), -1, 1, maximum, 1);
    }

    private void Transfer(bool deposit, int amount)
    {
        if (account is null || !Context.IsWorldReady || Game1.IsMultiplayer) return;
        int wallet = Game1.player.Money;
        bool success = deposit ? account.Deposit(amount, ref wallet) : account.Withdraw(amount, ref wallet);
        if (success)
            Game1.player.Money = wallet;
        Game1.exitActiveMenu();
        Say(success ? $"{(deposit ? "Dépôt" : "Retrait")} de {amount:N0} po effectué.^Solde placé : {account.Balance:N2} po."
            : "Opération impossible : le montant disponible a changé.");
    }

    private static void Say(string message) => Game1.drawObjectDialogue(message);
}

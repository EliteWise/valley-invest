# Valley Invest

Mod SMAPI pour Stardew Valley 1.6 sur ordinateur. Placez votre or auprès de Lewis et recevez des intérêts composés chaque nuit, sans ajouter de graphismes.

## Jouer

Dans cette première version, **Lewis fait office de conseiller lorsqu’il se trouve dans son manoir** (`ManorHouse`). Son emploi du temps reste celui du jeu : il n’est pas présent en permanence. Approchez-vous de lui et utilisez le bouton d’interaction habituel, à la souris, au clavier ou à la manette.

- **Déposer de l’or** : choisissez une somme entière avec le sélecteur natif. Le portefeuille est débité immédiatement.
- **Consulter mon compte** : affiche le solde, les intérêts cumulés, le dernier versement et le taux quotidien.
- **Retirer de l’or** : choisissez une somme, jusqu’à la totalité des pièces entières disponibles.
- **Parler à Lewis** : lance son interaction habituelle.

Le taux par défaut est de **1 % par nuit**. Les intérêts sont automatiquement réinvestis. Un dépôt de 10 000 po atteint **13 212,91 po après 28 nuits**, soit environ 13 213 po. Le calcul conserve huit décimales ; seuls les transferts sont limités aux pièces entières. Un retrait de toutes les pièces peut donc laisser une fraction de pièce, qui continue à produire des intérêts.

Le solde est plafonné à 2 147 483 647 po, comme un portefeuille représenté par un entier signé. Les intérêts sont limités au solde disponible avant ce plafond. Les intérêts cumulés restent consultables après les retraits.

**Version solo uniquement.** Les opérations et les intérêts sont désactivés en multijoueur. Le compte est propre à chaque sauvegarde ; il est enregistré avec la nuit de sauvegarde du jeu. Quitter sans sauvegarder annule les opérations de la journée, comme les changements du portefeuille. En cas de compte illisible ou invalide, le mod désactive les opérations et journalise l’erreur sans écraser les données.

## Installation

1. Installer SMAPI 4.0 ou supérieur pour Stardew Valley 1.6.
2. Décompresser `ValleyInvest 1.0.0.zip` dans le dossier `Mods` du jeu. Le dossier `Mods/ValleyInvest` doit contenir `manifest.json` et `ValleyInvest.dll`.
3. Lancer le jeu avec SMAPI et charger une partie solo.

Aucun autre mod, sprite ou portrait supplémentaire n’est requis.

## Configuration

Après le premier lancement, modifier `Mods/ValleyInvest/config.json` pendant que le jeu est fermé :

```json
{
  "DailyInterestRate": 0.01
}
```

`0.01` correspond à 1 %, `0` désactive les intérêts. La valeur doit être comprise entre `0` et `1` inclus. Une valeur numérique hors intervalle utilise 1 % avec un avertissement dans le journal SMAPI.

## Développement

Le mod cible .NET 6 et utilise le [paquet de compilation SMAPI](https://github.com/Pathoschild/SMAPI/tree/develop/src/SMAPI.ModBuildConfig). Installer Stardew Valley et SMAPI avant de compiler ; le paquet détecte normalement le dossier du jeu. Sinon, passer `-p:GamePath="/chemin/vers/Stardew Valley"` à la commande de compilation.

```sh
dotnet build -c Release
dotnet run --project tests/ValleyInvest.Tests.csproj
```

La compilation produit une archive installable dans `bin/Release/net6.0/`. Elle ne déploie pas automatiquement le mod dans le jeu. Les tests de calcul utilisent .NET 8 et ne nécessitent pas le jeu.

## Validation en jeu

La compilation et les tests automatisés du compte ne remplacent pas cette vérification manuelle :

1. En solo, parler à Lewis dans son manoir ; vérifier chaque option ainsi que « Parler à Lewis ».
2. Déposer 10 000 po, vérifier le débit, puis fermer le sélecteur sans valider une autre opération : aucun débit supplémentaire ne doit apparaître.
3. Dormir une nuit : le solde doit être de 10 100 po, dont 100 po d’intérêts. Recharger cette sauvegarde : les montants doivent rester identiques.
4. Retirer une partie puis toutes les pièces entières disponibles ; vérifier le crédit du portefeuille et l’historique des intérêts.
5. Charger une autre partie : son compte doit être indépendant. Vérifier aussi le menu à la manette et l’absence d’opérations en multijoueur.

Ces vérifications en jeu restent à effectuer.

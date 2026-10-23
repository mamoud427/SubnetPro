# SubnetPro

Boîte à outils **100 % hors ligne** pour les calculs d'adressage IPv4, développée en **.NET MAUI** (Android).

## Fonctionnalités

- **Sous-réseau** : adresse réseau, broadcast, masque, plage d'hôtes, nombre d'hôtes à partir d'une adresse IPv4 et d'un préfixe CIDR.
- **VLSM** : découpage d'un réseau selon les besoins en hôtes de chaque sous-réseau.
- **Classe & conversion** : classe IPv4 (A à E), conversion binaire ↔ décimal.
- **Historique** : calculs enregistrés sur l'appareil, séparés par compte.
- **Comptes locaux** : inscription et connexion sans serveur ; mots de passe hachés (PBKDF2-SHA256) ; aucune donnée ne quitte l'appareil.

## Architecture

- `SubnetPro` : application MAUI (MVVM avec CommunityToolkit.Mvvm, navigation Shell).
- `SubnetPro.Core` : logique métier sans dépendance MAUI (calculateurs, formatage, comptes, accès SQLite).
- `SubnetPro.Core.Tests` : tests xUnit de la logique métier.

Principes : MVVM, injection de dépendances, logique testable séparée de l'interface, stockage local SQLite (`sqlite-net-pcl`), session dans `SecureStorage`.

## Prérequis

- Visual Studio 2022 ou plus récent avec la charge de travail **.NET MAUI**
- .NET 9 SDK
- Un émulateur Android (testé sur Pixel 7, API 35)

## Lancer le projet

1. Cloner le dépôt et ouvrir la solution dans Visual Studio.
2. Choisir la cible **Android** et un émulateur, puis lancer.
3. Tests : *Test > Exécuter tous les tests*.

## Sécurité et confidentialité

- Aucune connexion réseau n'est utilisée.
- Les mots de passe ne sont jamais stockés en clair (PBKDF2-HMAC-SHA256, sel aléatoire, 210 000 itérations).
- Message d'erreur de connexion identique que l'identifiant existe ou non.

## Limites connues

- Pas de récupération de mot de passe (application hors ligne, comptes locaux).
- IPv4 uniquement.

## Auteur

Mamoud, projet réalisé en .NET MAUI.

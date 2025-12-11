# Système de Prédiction de Trajectoire Balistique

Ce document détaille le fonctionnement du script `PredictionManager.cs`, dont le rôle est de prédire en temps réel le point d'impact des projectiles (roquettes) sur le champ de bataille.

## Le Défi : Prédire sur un Terrain Irrégulier

La prédiction d'une trajectoire balistique simple (comme celle d'une roquette soumise uniquement à la gravité) est un problème de physique classique. L'équation du mouvement vertical est :

`y(t) = y₀ + v₀y * t + 0.5 * g * t²`

Où :
- `y(t)` est l'altitude à un temps `t`.
- `y₀` est l'altitude initiale.
- `v₀y` est la vitesse verticale initiale.
- `g` est l'accélération due à la gravité.
- `t` est le temps.

Le défi principal n'est pas l'équation elle-même, but de trouver la valeur de `t` pour laquelle `y(t)` correspond à l'altitude du sol. Sur une carte plate, c'est simple. Cependant, dans un environnement de jeu avec des collines, des vallées et des obstacles, l'altitude du sol varie constamment.

Calculer le temps d'impact en se basant sur une altitude moyenne (`y = 0` par exemple) mènerait à des erreurs de prédiction importantes : la roquette pourrait en réalité toucher une colline bien avant ou tomber dans un cratère bien après le temps estimé.

## La Stratégie : Une Prédiction Itérative en 3 Étapes

Pour résoudre ce problème, le `PredictionManager` utilise une approche itérative qui affine la prédiction :

1.  **Estimation Initiale** : Faire un premier calcul rapide en supposant que le sol est plat.
2.  **Correction de l'Altitude** : Trouver la véritable altitude du sol à l'endroit de ce premier impact estimé.
3.  **Calcul Final** : Recalculer la trajectoire avec cette altitude précise pour obtenir le point d'impact final.

---

### Étape 1 : Estimation Initiale (La " supposition éclairée ")

Le script commence par faire une première approximation.

- **Hypothèse** : Le sol est une surface plane située à l'altitude `m_GroundHeight` (une valeur moyenne, détectée au démarrage ou définie manuellement).
- **Calcul** : La méthode `CalculateImpactTime` résout l'équation du mouvement pour trouver `t` lorsque `y(t) = m_GroundHeight`. Cela nous donne un `estimatedTimeToImpact`.
- **Résultat** : En se basant sur ce temps et la vitesse horizontale de la roquette, le script calcule une position d'impact approximative `(estimatedImpactX, estimatedImpactZ)`.

Ce premier point est souvent incorrect si le terrain n'est pas plat, mais il nous donne une excellente zone de recherche pour l'étape suivante.

### Étape 2 : Correction de l'Altitude du Sol (Le " Raycast ")

Maintenant que nous avons une idée de l'endroit où la roquette *pourrait* atterrir, nous devons vérifier la topographie réelle à cet endroit.

- **Méthode** : Un `Physics.Raycast` est utilisé. Le script lance un rayon invisible tout droit vers le bas, depuis une altitude très élevée (la position actuelle de la roquette + 1000m, pour être sûr d'être au-dessus de n'importe quelle montagne) à la position horizontale `(estimatedImpactX, estimatedImpactZ)`.
- **Détection** : Si ce rayon touche le terrain, l'information `hit.point.y` nous donne l'altitude exacte du sol à cet endroit : `actualGroundHeight`.

### Étape 3 : Calcul Final (La Prédiction Affinée)

Armé de la bonne altitude du sol, le script peut maintenant faire un calcul final et précis.

- **Calcul** : La méthode `CalculateImpactTime` est appelée une seconde fois, mais cette fois-ci pour trouver `t` lorsque `y(t) = actualGroundHeight`. Cela produit un `finalTimeToImpact` beaucoup plus précis.
- **Résultat Final** : Le script calcule les coordonnées finales `finalImpactX` et `finalImpactZ` en utilisant ce temps de vol corrigé.

Le point d'impact final retourné est alors `(finalImpactX, actualGroundHeight, finalImpactZ)`, qui est une prédiction précise de l'endroit et de l'altitude où la roquette touchera le sol.

## Fonction Auxiliaire Clé : `CalculateImpactTime`

Cette méthode est le cœur mathématique de la prédiction.

- **Rôle** : Elle résout l'équation quadratique `(0.5*g)*t² + (v₀y)*t + (y₀ - targetY) = 0` pour trouver le temps `t`.
- **Robustesse** : Une équation quadratique peut avoir 0, 1 ou 2 solutions. Cette méthode gère tous les cas :
    - **Pas de solution** (`discriminant < 0`) : La roquette n'atteindra jamais l'altitude cible (par exemple, elle est tirée trop bas). La méthode retourne -1.
    - **Deux solutions positives** : Cela arrive si on cherche l'impact sur un sol plus bas que le point de départ. La roquette passe deux fois par la même altitude (une fois en montant, une fois en descendant). La méthode retourne la plus petite des deux solutions, car c'est le premier impact dans le futur.
    - **Une seule solution positive** : Le cas le plus courant.
    - **Aucune solution positive** : L'impact se serait produit dans le passé. La méthode retourne -1.

Cette approche garantit que nous obtenons toujours le premier impact plausible dans le futur.

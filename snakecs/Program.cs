using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace SnakeGame
{
    // --- 1. REPRÉSENTATION DU SERPENT ---
    class Snake
    {
        public List<Vector2> Corps { get; private set; }
        public Vector2 Direction { get; private set; }
        public bool Grandir { get; set; }

        public Snake(int colonnes, int lignes)
        {
            // Le serpent démarre au centre, avec 3 segments, allant vers la droite
            Vector2 centre = new Vector2(colonnes / 2, lignes / 2);
            Corps = new List<Vector2>
            {
                centre,
                new Vector2(centre.X - 1, centre.Y),
                new Vector2(centre.X - 2, centre.Y)
            };
            Direction = new Vector2(1, 0); // (dx, dy)
            Grandir = false;
        }

        public void ChangerDirection(Vector2 nouvelleDirection)
        {
            // On empêche de faire un demi-tour direct sur soi-même
            if (nouvelleDirection != -Direction)
            {
                Direction = nouvelleDirection;
            }
        }

        public void Avancer()
        {
            Vector2 tete = Corps[0];
            Vector2 nouvelleTete = tete + Direction;

            Corps.Insert(0, nouvelleTete);
            if (!Grandir)
            {
                Corps.RemoveAt(Corps.Count - 1); // On enlève la queue si on n'a pas mangé
            }
            else
            {
                Grandir = false;
            }
        }

        public bool ToucheMur(int colonnes, int lignes)
        {
            Vector2 tete = Corps[0];
            return tete.X < 0 || tete.X >= colonnes || tete.Y < 0 || tete.Y >= lignes;
        }

        public bool ToucheLuiMeme()
        {
            Vector2 tete = Corps[0];
            // On vérifie si la tête est présente dans le reste du corps
            for (int i = 1; i < Corps.Count; i++)
            {
                if (Corps[i] == tete) return true;
            }
            return false;
        }

        public bool Mange(Vector2 positionNourriture)
        {
            return Corps[0] == positionNourriture;
        }

        public void Dessiner(int tailleCase)
        {
            for (int i = 0; i < Corps.Count; i++)
            {
                // Tête verte claire, corps vert foncé
                Color couleur = (i == 0) ? new Color(0, 200, 0, 255) : new Color(0, 130, 0, 255);
                
                int x = (int)Corps[i].X * tailleCase;
                int y = (int)Corps[i].Y * tailleCase;

                // Dessin du carré coloré
                Raylib.DrawRectangle(x, y, tailleCase, tailleCase, couleur);
                // Petit contour noir tout autour
                Raylib.DrawRectangleLines(x, y, tailleCase, tailleCase, new Color(20, 20, 20, 255));
            }
        }
    }

    // --- 2. LOGIQUE PRINCIPALE DU PROGRAMME ---
    class Program
    {
        // Configuration identique à votre fichier Python
        const int TAILLE_CASE = 20;
        const int LARGEUR = 600;
        const int HAUTEUR = 400;
        const int COLONNES = LARGEUR / TAILLE_CASE;
        const int LIGNES = HAUTEUR / TAILLE_CASE;
        const int VITESSE_DEPART = 8;

        // Couleurs de fond et de nourriture
        static readonly Color NOIR_FOND = new Color(20, 20, 20, 255);
        static readonly Color ROUGE_POMME = new Color(200, 30, 30, 255);

        // Variables d'état du jeu
        static Snake snake;
        static Vector2 nourriture;
        static int score;
        static int vitesse;
        static bool jeuTermine;
        static Random random = new Random();

        static void Main()
        {
            // Initialisation de la fenêtre de jeu
            Raylib.InitWindow(LARGEUR, HAUTEUR, "Snake en C# (Raylib)");
            
            NouvellePartie();

            // Boucle principale
            while (!Raylib.WindowShouldClose())
            {
                // Gestion de la vitesse dynamique (FPS adaptatif)
                Raylib.SetTargetFPS(vitesse);

                // --- 1. Gestion des Événements Clavier ---
                if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                {
                    break; // Quitte le jeu
                }

                if (jeuTermine && Raylib.IsKeyPressed(KeyboardKey.Space))
                {
                    NouvellePartie();
                }
                else if (!jeuTermine)
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.Up))    snake.ChangerDirection(new Vector2(0, -1));
                    if (Raylib.IsKeyPressed(KeyboardKey.Down))  snake.ChangerDirection(new Vector2(0, 1));
                    if (Raylib.IsKeyPressed(KeyboardKey.Left))  snake.ChangerDirection(new Vector2(-1, 0));
                    if (Raylib.IsKeyPressed(KeyboardKey.Right)) snake.ChangerDirection(new Vector2(1, 0));
                }

                // --- 2. Mise à jour de la logique du jeu ---
                if (!jeuTermine)
                {
                    snake.Avancer();

                    if (snake.Mange(nourriture))
                    {
                        snake.Grandir = true;
                        score++;
                        nourriture = PositionAleatoireLibre(snake.Corps);
                        // Le jeu accélère légèrement tous les 3 bonbons mangés
                        vitesse = VITESSE_DEPART + (score / 3);
                    }

                    if (snake.ToucheMur(COLONNES, LIGNES) || snake.ToucheLuiMeme())
                    {
                        jeuTermine = true;
                    }
                }

                // --- 3. Affichage ---
                Raylib.BeginDrawing();
                Raylib.ClearBackground(NOIR_FOND);

                if (!jeuTermine)
                {
                    // Dessiner Nourriture
                    Raylib.DrawRectangle((int)nourriture.X * TAILLE_CASE, (int)nourriture.Y * TAILLE_CASE, TAILLE_CASE, TAILLE_CASE, ROUGE_POMME);
                    
                    // Dessiner Serpent
                    snake.Dessiner(TAILLE_CASE);

                    // Afficher le Score
                    Raylib.DrawText($"Score : {score}", 10, 10, 24, Color.White);
                }
                else
                {
                    // Écran Game Over (Textes centrés)
                    Raylib.DrawText("GAME OVER", LARGEUR / 2 - 120, HAUTEUR / 2 - 50, 40, ROUGE_POMME);
                    Raylib.DrawText($"Score final : {score}", LARGEUR / 2 - 60, HAUTEUR / 2, 20, Color.White);
                    Raylib.DrawText("Appuie sur ESPACE pour rejouer", LARGEUR / 2 - 140, HAUTEUR / 2 + 40, 20, Color.White);
                }

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }

        // Réinitialise une nouvelle partie
        static void NouvellePartie()
        {
            snake = new Snake(COLONNES, LIGNES);
            nourriture = PositionAleatoireLibre(snake.Corps);
            score = 0;
            vitesse = VITESSE_DEPART;
            jeuTermine = false;
        }

        // Trouve une case libre pour placer la nourriture (pas sur le serpent)
        static Vector2 PositionAleatoireLibre(List<Vector2> corpsSerpent)
        {
            while (true)
            {
                Vector2 pos = new Vector2(random.Next(0, COLONNES), random.Next(0, LIGNES));
                if (!corpsSerpent.Contains(pos))
                {
                    return pos;
                }
            }
        }
    }
}

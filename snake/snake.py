"""
Snake - version simple avec Pygame
Commandes : flèches directionnelles pour bouger, ECHAP pour quitter
"""

import pygame
import random
import sys

# --- Configuration ---
TAILLE_CASE = 20
LARGEUR, HAUTEUR = 600, 400
COLONNES = LARGEUR // TAILLE_CASE
LIGNES = HAUTEUR // TAILLE_CASE
VITESSE_DEPART = 8  # cases par seconde

# Couleurs (R, G, B)
NOIR = (20, 20, 20)
VERT = (0, 200, 0)
VERT_FONCE = (0, 130, 0)
ROUGE = (200, 30, 30)
BLANC = (255, 255, 255)


class Snake:
    """Représente le serpent : sa position et son comportement."""

    def __init__(self):
        # Le serpent démarre au centre, avec 3 segments, allant vers la droite
        centre = (COLONNES // 2, LIGNES // 2)
        self.corps = [centre, (centre[0] - 1, centre[1]), (centre[0] - 2, centre[1])]
        self.direction = (1, 0)  # (dx, dy)
        self.grandir = False

    def changer_direction(self, nouvelle_direction):
        # On empêche de faire un demi-tour direct sur soi-même
        dx, dy = nouvelle_direction
        ancien_dx, ancien_dy = self.direction
        if (dx, dy) != (-ancien_dx, -ancien_dy):
            self.direction = nouvelle_direction

    def avancer(self):
        tete_x, tete_y = self.corps[0]
        dx, dy = self.direction
        nouvelle_tete = (tete_x + dx, tete_y + dy)

        self.corps.insert(0, nouvelle_tete)
        if not self.grandir:
            self.corps.pop()  # on enlève la queue si on n'a pas mangé
        else:
            self.grandir = False

    def touche_mur(self):
        x, y = self.corps[0]
        return x < 0 or x >= COLONNES or y < 0 or y >= LIGNES

    def touche_lui_meme(self):
        return self.corps[0] in self.corps[1:]

    def mange(self, position_nourriture):
        return self.corps[0] == position_nourriture

    def dessiner(self, ecran):
        for i, (x, y) in enumerate(self.corps):
            couleur = VERT if i == 0 else VERT_FONCE  # tête plus claire
            rect = pygame.Rect(x * TAILLE_CASE, y * TAILLE_CASE, TAILLE_CASE, TAILLE_CASE)
            pygame.draw.rect(ecran, couleur, rect)
            pygame.draw.rect(ecran, NOIR, rect, 1)  # petit contour


def position_aleatoire_libre(corps):
    """Trouve une case libre pour placer la nourriture (pas sur le serpent)."""
    while True:
        pos = (random.randint(0, COLONNES - 1), random.randint(0, LIGNES - 1))
        if pos not in corps:
            return pos


def dessiner_nourriture(ecran, position):
    x, y = position
    rect = pygame.Rect(x * TAILLE_CASE, y * TAILLE_CASE, TAILLE_CASE, TAILLE_CASE)
    pygame.draw.rect(ecran, ROUGE, rect)


def afficher_score(ecran, score, police):
    texte = police.render(f"Score : {score}", True, BLANC)
    ecran.blit(texte, (10, 10))


def ecran_game_over(ecran, score, police_grande, police_petite):
    ecran.fill(NOIR)
    texte1 = police_grande.render("GAME OVER", True, ROUGE)
    texte2 = police_petite.render(f"Score final : {score}", True, BLANC)
    texte3 = police_petite.render("Appuie sur ESPACE pour rejouer", True, BLANC)

    ecran.blit(texte1, texte1.get_rect(center=(LARGEUR // 2, HAUTEUR // 2 - 40)))
    ecran.blit(texte2, texte2.get_rect(center=(LARGEUR // 2, HAUTEUR // 2)))
    ecran.blit(texte3, texte3.get_rect(center=(LARGEUR // 2, HAUTEUR // 2 + 40)))


def main():
    pygame.init()
    ecran = pygame.display.set_mode((LARGEUR, HAUTEUR))
    pygame.display.set_caption("Snake")
    horloge = pygame.time.Clock()
    police = pygame.font.SysFont("arial", 24)
    police_grande = pygame.font.SysFont("arial", 48, bold=True)

    def nouvelle_partie():
        s = Snake()
        n = position_aleatoire_libre(s.corps)
        return s, n, 0, VITESSE_DEPART

    snake, nourriture, score, vitesse = nouvelle_partie()
    jeu_termine = False

    en_cours = True
    while en_cours:
        # --- Gestion des événements ---
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                en_cours = False
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    en_cours = False
                elif jeu_termine and event.key == pygame.K_SPACE:
                    snake, nourriture, score, vitesse = nouvelle_partie()
                    jeu_termine = False
                elif not jeu_termine:
                    if event.key == pygame.K_UP:
                        snake.changer_direction((0, -1))
                    elif event.key == pygame.K_DOWN:
                        snake.changer_direction((0, 1))
                    elif event.key == pygame.K_LEFT:
                        snake.changer_direction((-1, 0))
                    elif event.key == pygame.K_RIGHT:
                        snake.changer_direction((1, 0))

        if not jeu_termine:
            # --- Mise à jour de l'état du jeu ---
            snake.avancer()

            if snake.mange(nourriture):
                snake.grandir = True
                score += 1
                nourriture = position_aleatoire_libre(snake.corps)
                # Le jeu accélère légèrement à chaque bonbon mangé
                vitesse = VITESSE_DEPART + score // 3

            if snake.touche_mur() or snake.touche_lui_meme():
                jeu_termine = True

            # --- Affichage ---
            ecran.fill(NOIR)
            dessiner_nourriture(ecran, nourriture)
            snake.dessiner(ecran)
            afficher_score(ecran, score, police)
        else:
            ecran_game_over(ecran, score, police_grande, police)

        pygame.display.flip()
        horloge.tick(vitesse)

    pygame.quit()
    sys.exit()


if __name__ == "__main__":
    main()
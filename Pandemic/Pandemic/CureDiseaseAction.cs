﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandemic
{
    public class CureDiseaseAction : Action
    {
        DiseaseColor color;
        private CureDiseaseAction(DiseaseColor color)
        {
            this.color = color;
        }

        public override GameState execute(GameState gs)
        {
            Player player = gs.currentPlayer();
            int cardsRemoved = 0;

            foreach (City card in player.cards)
            {
                if (cardsRemoved == 5 || (cardsRemoved == 4 && gs.currentPlayer().type == Player.Type.SCIENTIST)) break;
                if (color == card.color)
                {
                    player = player.removeCard(card);
                    cardsRemoved++;
                }
            }
            GameState result = gs.cureDisease(color);
            result = result.adjustPlayer(player);
            result.advanceMove();
            result = result.recalcBestCardHolder(result, gs.currentPlayer(), color);
            return result;

        }

        public static List<CureDiseaseAction> getCureActions(GameState g)
        {
            List<CureDiseaseAction> result = new List<CureDiseaseAction>();
            if(g.map.hasStation(g.currentPlayer().position)) {
                foreach(DiseaseColor c in g.currentPlayer().hasCardsToCure())
                {
                    if (g.curesFound[(int) c]) continue;

                    result.Add(new CureDiseaseAction(c));
                }
            }
            return result;
        }

        public override string ToString()
        {
            return "Cure " + color.ToString() + " disease";
        }
    }
}

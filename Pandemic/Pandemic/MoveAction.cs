﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pandemic
{
    public class MoveAction : Action
    {

        Player player;
        City dest;

        public MoveAction(Player player, City dest)
        {
            this.player = player;
            this.dest = dest;
        }

        public override GameState execute(GameState gs)
        {

            GameState result = new GameState(gs);
            player = new Player(dest);
            result.player = player;
            return result;
        }
    }
}

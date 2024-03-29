﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Pandemic
{
    public class CureCityAction : Action
    {

        City position;
        DiseaseColor color;

        public CureCityAction(City position, DiseaseColor color)
        {
            this.position = position;
            this.color = color;
        }

        public override GameState execute(GameState current)
        {
            Debug.Assert(debug_gs == null || debug_gs == current, "Action used on an unintended gamestate");
            Map newMap = current.map.removeDisease(position, color);
            return new GameState(current, newMap);
        }

        public override string ToString()
        {
            return "Treat " + position.name;
        }
    }
}

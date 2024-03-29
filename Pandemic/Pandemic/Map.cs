﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Pandemic
{
    public class Map
    {

        private class CityData
        {
            private int[] diseases;
            private bool station = false;
            public List<MoveAction> moveActions;

            public CityData()
            {
                diseases = new int[4];
                diseases[(int)DiseaseColor.BLACK] = 0;
                diseases[(int)DiseaseColor.BLUE] = 0;
                diseases[(int)DiseaseColor.YELLOW] = 0;
                diseases[(int)DiseaseColor.ORANGE] = 0;
            }

            public CityData(CityData other)
            {
                diseases = new int[4];
                diseases[(int)DiseaseColor.BLACK] = other.diseases[(int)DiseaseColor.BLACK];
                diseases[(int)DiseaseColor.BLUE] = other.diseases[(int)DiseaseColor.BLUE];
                diseases[(int)DiseaseColor.YELLOW] = other.diseases[(int)DiseaseColor.YELLOW];
                diseases[(int)DiseaseColor.ORANGE] = other.diseases[(int)DiseaseColor.ORANGE];
                station = other.station;
                moveActions = other.moveActions;
            }

            public int disease(DiseaseColor color)
            {
                return diseases[(int)color];
            }

            public bool hasStation()
            {
                return station;
            }

            public CityData adjustDisease(DiseaseColor color, int adjustment)
            {
                CityData result = new CityData(this);
                int newValue = result.diseases[(int)color] + adjustment;
                Debug.Assert(newValue <= 3 && newValue >= 0);
                result.diseases[(int)color] = newValue;
                return result;
            }

            public CityData addStation()
            {
                Debug.Assert(!station);
                CityData result = new CityData(this);
                result.station = true;
                return result;
            }

        }

        private CityData[] cities;
        public List<City> aboutToOutbreak;
        public int numInfectionsInCities = 0;
        private int _outbreakCount = 0;
        public int numStations = 0;
        public int infectionRate = 0; //spot on board not num cards to draw
        //dont modify the station list
        public List<City> stations;
        public List<City> allCities;
        private int numCities;

        public int outbreakCount
        {
            get { return _outbreakCount; }
        }

        public Map()
        {
            cities = new CityData[0];
            stations = new List<City>();
            aboutToOutbreak = new List<City>();
            allCities = new List<City>();
        }

        public Map(Map oldMap)
        {
            numCities = oldMap.numCities;
            cities = new CityData[numCities];
            for (int i = 0; i < numCities; i++)
            {
                cities[i] = oldMap.cities[i];
            }
            allCities = oldMap.allCities;
            _outbreakCount = oldMap.outbreakCount;
            stations = oldMap.stations;
            aboutToOutbreak = oldMap.aboutToOutbreak;
            numStations = oldMap.numStations;
            infectionRate = oldMap.infectionRate;
            numInfectionsInCities = oldMap.numInfectionsInCities;            
        }

        //dangerous...modifies the Map
        public Map addDisease(City c, int num = 1)
        {
            Map result = new Map(this);
            DiseaseColor color = c.color;

            for (int i = 0; i < num; i++)
            {

                if (result.cities[c.cityNumber].disease(color) < 3)
                {
                    result.cities[c.cityNumber] = result.cities[c.cityNumber].adjustDisease(color, 1);

                    if (result.diseaseLevel(c, color) == 3)
                    {
                        result.aboutToOutbreak = new List<City>(result.aboutToOutbreak);
                        result.aboutToOutbreak.Add(c);
                    }

                    result.numInfectionsInCities ++;
                }
                else
                {
                    //outbreak
                    List<City> outbreaks = new List<City>();
                    outbreaks.Add(c);
                    List<City> toEvaluate = new List<City>();
                    toEvaluate.AddRange(c.adjacent);
                    while (toEvaluate.Count != 0)
                    {
                        City current = toEvaluate.ElementAt(0);
                        toEvaluate.RemoveAt(0);
                        if (outbreaks.Contains(current))
                            continue;
                        if (result.cities[current.cityNumber].disease(color) < 3)
                        {
                            result.cities[current.cityNumber] = result.cities[current.cityNumber].adjustDisease(color, 1);
                        }
                        else
                        {
                            toEvaluate.AddRange(current.adjacent);
                            outbreaks.Add(current);
                        }

                        result.numInfectionsInCities++;

                        if (result.diseaseLevel(current, color) == 3)
                        {
                            result.aboutToOutbreak = new List<City>(result.aboutToOutbreak);
                            result.aboutToOutbreak.Add(current);
                        }
                    }
                    result._outbreakCount += outbreaks.Count;
                }
            }
            return result;
        }

        public Map removeDisease(City city, DiseaseColor color)
        {
            Map result = new Map(this);
            result.cities[city.cityNumber] = result.cities[city.cityNumber].adjustDisease(color, -1);

            result.numInfectionsInCities --;

            if (result.aboutToOutbreak.Contains(city) && result.diseaseLevel(city, city.color) != 3)
            {
                result.aboutToOutbreak = new List<City>(result.aboutToOutbreak);
                result.aboutToOutbreak.Remove(city);
            }

            return result;
        }

        public Map addStation(City city)
        {
            Map result = new Map(this);
            result.stations.Add(city);
            result.cities[city.cityNumber] = cities[city.cityNumber].addStation();
            result.cities[city.cityNumber].moveActions = null;
            return result;
        }

        public int diseaseLevel(City city, DiseaseColor color)
        {
            return cities[city.cityNumber].disease(color);
        }

        public City addCity(String name, DiseaseColor color, float x = 0f, float y = 0f)
        {            
            City city = new City(name, color, numCities, x, y);
            numCities++;

            CityData[] newData = new CityData[numCities];
            for (int i = 0; i < numCities - 1; i++)
            {
                newData[i] = cities[i];
            }
            cities = newData;
            cities[city.cityNumber] = new CityData();
            allCities.Add(city);
            return city;
        }

        public bool hasStation(City city)
        {
            return cities[city.cityNumber].hasStation();
        }


        public List<MoveAction> getMoveActionsFor(Player player)
        {
            if (cities[player.position.cityNumber].moveActions == null)
            {
                List<MoveAction> moves = new List<MoveAction>();
                foreach (City c in player.position.adjacent)
                {
                    moves.Add(new MoveAction(c));
                }
                if (cities[player.position.cityNumber].hasStation())
                {
                    foreach (City c in stations)
                    {
                        if (c == player.position)
                            continue;
                        // this turns out to be slow
                        if (player.position.isAdjacent(c))
                            continue;
                        moves.Add(new MoveAction(c));
                    }
                }
                cities[player.position.cityNumber].moveActions = moves;

            }
            return cities[player.position.cityNumber].moveActions;
        }

        public List<CureCityAction> getCureActionsFor(Player player)
        {
            List<CureCityAction> cures = new List<CureCityAction>();
            for (int i = 0; i < 4; i++)
            {
                DiseaseColor color = (DiseaseColor)i;
                if (diseaseLevel(player.position, color) > 0)
                {
                    cures.Add(new CureCityAction(player.position, color));
                }
            }
            return cures;
        }
    }
}

using System;
using System.Linq;
using ASD.Graphs;

namespace ASD
{
    public class ProductionPlanner : MarshalByRefObject
    {
        /// <summary>
        /// Flaga pozwalająca na włączenie wypisywania szczegółów skonstruowanego planu na konsolę.
        /// Wartość <code>true</code> spoeoduje wypisanie planu.
        /// </summary>
        public bool ShowDebug { get; } = false;

        /// <summary>
        /// Część 1. zadania - zaplanowanie produkcji telewizorów dla pojedynczego kontrahenta.
        /// </summary>
        /// <remarks>
        /// Do przeprowadzenia testów wyznaczających maksymalną produkcję i zysk wymagane jest jedynie zwrócenie obiektu <see cref="PlanData"/>.
        /// Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan"/>.
        /// </remarks>
        /// <param name="production">
        /// Tablica obiektów zawierających informacje o produkcji fabryki w kolejnych tygodniach.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają limit produkcji w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - koszt produkcji jednej sztuki.
        /// </param>
        /// <param name="sales">
        /// Tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają maksymalną sprzedaż w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - cenę sprzedaży jednej sztuki.
        /// </param>
        /// <param name="storageInfo">
        /// Obiekt zawierający informacje o magazynie.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza pojemność magazynu,
        /// a pola <see cref="PlanData.Value"/> - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
        /// </param>
        /// <param name="weeklyPlan">
        /// Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
        /// </param>
        /// <returns>
        /// Obiekt <see cref="PlanData"/> opisujący wyznaczony plan.
        /// W polu <see cref="PlanData.Quantity"/> powinna znaleźć się maksymalna liczba wyprodukowanych telewizorów,
        /// a w polu <see cref="PlanData.Value"/> - wyznaczony maksymalny zysk fabryki.
        /// </returns>
        public PlanData CreateSimplePlan(PlanData[] production, PlanData[] sales, PlanData storageInfo,
            out SimpleWeeklyPlan[] weeklyPlan)
        {
            int dayCount = production.Length;
            NetworkWithCosts<int, double> model = new NetworkWithCosts<int, double>(dayCount + 2);
            for (int i = 0; i < dayCount; i++)
            {
                model.AddEdge(dayCount, i, production[i].Quantity, production[i].Value);
                model.AddEdge(i, dayCount + 1, sales[i].Quantity, -sales[i].Value);
                if (i > 0)
                {
                    model.AddEdge(i - 1, i, storageInfo.Quantity, storageInfo.Value);
                }
            }

            weeklyPlan = new SimpleWeeklyPlan[dayCount];

            var (flowVal, flowCost, f) = Flows.MinCostMaxFlow(model, dayCount, dayCount + 1);

            for (int i = 0; i < dayCount; i++)
            {
                if (f.HasEdge(dayCount, i))
                {
                    weeklyPlan[i].UnitsProduced = f.GetEdgeWeight(dayCount, i);
                }
                else
                {
                    weeklyPlan[i].UnitsProduced = 0;
                }

                if (f.HasEdge(i, dayCount + 1))
                {
                    weeklyPlan[i].UnitsSold = f.GetEdgeWeight(i, dayCount + 1);
                }
                else
                {
                    weeklyPlan[i].UnitsSold = 0;
                }

                if (i < dayCount - 1)
                {
                    if (f.HasEdge(i, i + 1))
                    {
                        weeklyPlan[i].UnitsStored = f.GetEdgeWeight(i, i + 1);
                    }
                    else
                    {
                        weeklyPlan[i].UnitsStored = 0;
                    }
                }
                else
                {
                    weeklyPlan[i].UnitsStored = 0;
                }
            }


            return new PlanData
            {
                Value = -flowCost,
                Quantity = flowVal
            };
        }

        /// <summary>
        /// Część 2. zadania - zaplanowanie produkcji telewizorów dla wielu kontrahentów.
        /// </summary>
        /// <remarks>
        /// Do przeprowadzenia testów wyznaczających produkcję dającą maksymalny zysk wymagane jest jedynie zwrócenie obiektu <see cref="PlanData"/>.
        /// Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan"/>.
        /// </remarks>
        /// <param name="production">
        /// Tablica obiektów zawierających informacje o produkcji fabryki w kolejnych tygodniach.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza limit produkcji w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - koszt produkcji jednej sztuki.
        /// </param>
        /// <param name="sales">
        /// Dwuwymiarowa tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach.
        /// Pierwszy wymiar tablicy jest równy liczbie kontrahentów, zaś drugi - liczbie tygodni w planie.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają maksymalną sprzedaż w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - cenę sprzedaży jednej sztuki.
        /// Każdy wiersz tablicy odpowiada jednemu kontrachentowi.
        /// </param>
        /// <param name="storageInfo">
        /// Obiekt zawierający informacje o magazynie.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza pojemność magazynu,
        /// a pola <see cref="PlanData.Value"/> - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
        /// </param>
        /// <param name="weeklyPlan">
        /// Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
        /// </param>
        /// <returns>
        /// Obiekt <see cref="PlanData"/> opisujący wyznaczony plan.
        /// W polu <see cref="PlanData.Quantity"/> powinna znaleźć się optymalna liczba wyprodukowanych telewizorów,
        /// a w polu <see cref="PlanData.Value"/> - wyznaczony maksymalny zysk fabryki.
        /// </returns>
        public PlanData CreateComplexPlan(PlanData[] production, PlanData[,] sales, PlanData storageInfo,
            out WeeklyPlan[] weeklyPlan)
        {
            int dayCount = production.Length;
            int clientCount = sales.GetLength(0);
            NetworkWithCosts<int, double> model = new NetworkWithCosts<int, double>(dayCount + 3 + clientCount);
            for (int i = 0; i < dayCount; i++)
            {
                model.AddEdge(dayCount + clientCount, i, production[i].Quantity, production[i].Value);
                for (int j = 0; j < clientCount; j++)
                {
                    model.AddEdge(i, dayCount + j, sales[j, i].Quantity, -sales[j, i].Value);
                }

                if (i > 0)
                {
                    model.AddEdge(i - 1, i, storageInfo.Quantity, storageInfo.Value);
                }
            }

            int maxProduction = 0;
            for (int i = 0; i < dayCount; i++)
            {
                maxProduction += production[i].Quantity;
            }

            model.AddEdge(dayCount + clientCount + 1, dayCount + clientCount, maxProduction, 0);
            for (int i = 0; i < clientCount; i++)
            {
                model.AddEdge(dayCount + i, dayCount + clientCount + 2, Int32.MaxValue, 0);
            }

            model.AddEdge(dayCount + clientCount, dayCount + clientCount + 2, Int32.MaxValue, 0);

            weeklyPlan = new WeeklyPlan[dayCount];
            // weeklyPlan = null;

            var (flowVal, flowCost, f) =
                Flows.MinCostMaxFlow(model, dayCount + clientCount + 1, clientCount + dayCount + 2);

            for (int i = 0; i < dayCount; i++)
            {
                if (f.HasEdge(dayCount + clientCount, i))
                {
                    weeklyPlan[i].UnitsProduced = f.GetEdgeWeight(dayCount + clientCount, i);
                }
                else
                {
                    weeklyPlan[i].UnitsProduced = 0;
                }

                weeklyPlan[i].UnitsSold = new int[clientCount];
                for (int j = 0; j < clientCount; j++)
                {
                    if (f.HasEdge(i, dayCount + j))
                    {
                        weeklyPlan[i].UnitsSold[j] = f.GetEdgeWeight(i, dayCount + j);
                    }
                    else
                    {
                        weeklyPlan[i].UnitsSold[j] = 0;
                    }
                }

             
                if (i < dayCount - 1)
                {
                    if (f.HasEdge(i, i + 1))
                    {
                        weeklyPlan[i].UnitsStored = f.GetEdgeWeight(i, i + 1);
                    }
                    else
                    {
                        weeklyPlan[i].UnitsStored = 0;
                    }
                }
                else
                {
                    weeklyPlan[i].UnitsStored = 0;
                }
            }


            return new PlanData
            {
                Value = -flowCost,
                Quantity = flowVal
            };
        }
    }
}
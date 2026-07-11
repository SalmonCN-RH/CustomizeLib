using UnityEngine;

namespace ClassLibrary1
{
    public class Class1
    {
        public static void Test()
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                foreach (var plant in Lawnf.GetAllPlants())
                {
                    if (plant == null) continue;
                    if (plant.thePlantType != PlantType.ScaredyShroom) continue;
                    CreatePlant.Instance.SetPlant(plant.thePlantColumn + 1, plant.thePlantRow, PlantType.ScaredyShroom, plant, isFreeSet: true);
                }
            }
            AlmanacData.PlantInfo data = AlmanacData.AlmanacDataLoader.plantDatas[PlantType.Peashooter];
            Console.WriteLine($"\n" +
                $"{data.name}\n" +
                $"{data.info}\n" +
                $"{data.introduce}");
        }
    }
}

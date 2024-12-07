// NutritionCalculator.cs
using Assignment3.Models;
using System.Linq;

public class NutritionCalculator
{
    public static int CalculateBMR(Person person)
    {
        // Mifflin-St Jeor Formula
        float weight = person.Weight ?? 0;
        int age = person.Age;
        bool isMale = person.Sex?.ToLower() == "male";

        double bmr = (10 * weight) + (6.25 * 170) - (5 * age);
        bmr = isMale ? bmr + 5 : bmr - 161;

        return (int)bmr;
    }

    public static int CalculateAverageRoutineCaloriesBurned(Person person)
    {
        if (person.Routines == null || !person.Routines.Any())
            return 0;

        double totalWeeklyCaloriesBurned = 0;

        foreach (var routine in person.Routines)
        {
            if (routine.Exercises == null || !routine.Exercises.Any())
                continue;

            foreach (var exercise in routine.Exercises)
            {
                int caloriesBurned = EstimateCaloriesBurned(exercise, person.Weight ?? 0);
                totalWeeklyCaloriesBurned += caloriesBurned * exercise.Sets;
            }
        }

        // Average daily calories burned
        int averageDailyCaloriesBurned = (int)(totalWeeklyCaloriesBurned / 7);

        return averageDailyCaloriesBurned;
    }

    private static int EstimateCaloriesBurned(Exercise exercise, float weight)
    {
        // MET value for the exercise
        double metValue = GetMetValue(exercise.Name ?? string.Empty);
        // Assume average duration per set is 5 minutes
        int durationMinutes = 5 * exercise.Reps;
        double caloriesBurned = (metValue * 3.5 * weight) / 200 * durationMinutes;
        return (int)caloriesBurned;
    }

    private static double GetMetValue(string exerciseName)
    {
        // Return MET value based on exercise name
        return exerciseName.ToLower() switch
        {
            "running" => 9.8,
            "cycling" => 7.5,
            "squats" => 5.0,
            "bench press" => 6.0,
            // Add more exercises as needed
            _ => 5.0, // Default MET value
        };
    }

    public static int CalculateDailyCalorieAdjustment(Person person)
    {
        float currentWeight = person.Weight ?? 0;
        float goalWeight = person.GoalWeight ?? 0;
        int timeFrameWeeks = person.Time;

        float weightDifference = goalWeight - currentWeight;
        float weeklyChange = weightDifference / timeFrameWeeks;

        // Each pound (~0.45 kg) is approximately 3500 calories
        int dailyCalorieChange = (int)(weeklyChange * 3500 / 7);

        return dailyCalorieChange;
    }

    public static int GetTotalDailyCalories(Person person)
    {
        int bmr = CalculateBMR(person);
        int averageRoutineCaloriesBurned = CalculateAverageRoutineCaloriesBurned(person) / 7;
        int dailyCalorieAdjustment = CalculateDailyCalorieAdjustment(person);

        int totalDailyCalories = bmr +  + dailyCalorieAdjustment;
        return totalDailyCalories;
    }

    public static (int protein, int carbs, int fat) CalculateMacroPercentages(Person person)
    {
        bool isGaining = (person.GoalWeight ?? 0) > (person.Weight ?? 0);

        if (isGaining)
        {
            return (protein: 30, carbs: 50, fat: 20); // High carb for bulking
        }
        else
        {
            return (protein: 40, carbs: 30, fat: 30); // High protein for cutting
        }
    }
}
// NutritionCalculator.cs
using Assignment3.Models;
using Microsoft.CodeAnalysis;
using System.Linq;

public class NutritionCalculator
{
    public static int CalculateBMR(Person person)
    {
        double bmr;
        float weight = person.Weight ?? 0;
        float height = person.Height ?? 0;
        int age = person.Age;
        bool isMale = person.Sex?.ToLower() == "male";

        // Mifflin-St Jeor Equation
        bmr = (10 * 0.453592 * weight) + (6.25 * 2.54 * height) - (5 * age);
        if (isMale)
        {
            bmr += 5;
        }
        else
        {
            bmr -= 161;
        }

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
        float dailyChange = weeklyChange / 7;

        // Each pound (~0.45 kg) is approximately 3500 calories
        int dailyCalorieAdjustment = (int)(dailyChange * 3500);

        return dailyCalorieAdjustment;
    }

    public static int GetTotalDailyCalories(Person person)
    {
        int bmr = CalculateBMR(person);
        int dailyCalorieAdjustment = CalculateDailyCalorieAdjustment(person);

        // 1.6 is activity multiplier - takes bmr to calculate actual calories burned per day
        int totalDailyCalories = (int)((bmr * 1.6) + dailyCalorieAdjustment);
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

    public static (int breakfastCalories, int lunchCalories, int dinnerCalories) CalculateMealCalories(int totalDailyCalories)
    {
        int breakfastCalories = (int)(totalDailyCalories * 0.25);
        int lunchCalories = (int)(totalDailyCalories * 0.30);
        int dinnerCalories = (int)(totalDailyCalories * 0.45);

        return (breakfastCalories, lunchCalories, dinnerCalories);
    }
}
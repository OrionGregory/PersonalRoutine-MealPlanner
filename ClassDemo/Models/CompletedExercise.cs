namespace Assignment3.Models;
public class CompletedExercise
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; }
    public DateTime CompletedDate { get; set; }
    public string? UserId { get; set; }
}
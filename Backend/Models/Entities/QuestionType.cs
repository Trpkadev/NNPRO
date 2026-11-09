namespace Backend.Models.Entities;

public enum QuestionType : byte {
    SingleChoice = 0, //Jednoznačná volba
    MultipleChoice = 1, // Výběr více odpovědí
    TrueFalse = 2 // Pravda / nepravda
}
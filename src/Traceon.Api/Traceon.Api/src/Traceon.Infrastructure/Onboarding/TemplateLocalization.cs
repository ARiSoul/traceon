namespace Traceon.Infrastructure.Onboarding;

/// <summary>
/// Provides localized text for template names, descriptions, tags, and dropdown values
/// so that templates are installed in the user's language.
/// </summary>
public static class TemplateLocalization
{
    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["pt"] = new(StringComparer.OrdinalIgnoreCase)
        {
            // Tags
            ["Template_Tag_Health"] = "Saúde",
            ["Template_Tag_Vitals"] = "Sinais Vitais",
            ["Template_Tag_Fitness"] = "Fitness",
            ["Template_Tag_Exercise"] = "Exercício",
            ["Template_Tag_Habits"] = "Hábitos",
            ["Template_Tag_Wellness"] = "Bem-estar",
            ["Template_Tag_Finance"] = "Finanças",

            // Health
            ["Template_Health_BloodPressure"] = "Pressão Arterial",
            ["Template_Health_BloodPressure_Desc"] = "Registe sistólica, diastólica e pulso.",
            ["Template_Health_Weight"] = "Peso",
            ["Template_Health_Weight_Desc"] = "Acompanhe o peso corporal ao longo do tempo.",
            ["Template_Health_BloodGlucose"] = "Glicose no Sangue",
            ["Template_Health_BloodGlucose_Desc"] = "Monitorize os níveis de açúcar no sangue.",

            // Fitness
            ["Template_Fitness_Workout"] = "Treino",
            ["Template_Fitness_Workout_Desc"] = "Registe tipo de exercício, duração e calorias.",
            ["Template_Fitness_Running"] = "Corrida",
            ["Template_Fitness_Running_Desc"] = "Registe distância e tempo.",
            ["Template_Fitness_Steps"] = "Passos",
            ["Template_Fitness_Steps_Desc"] = "Conte os seus passos diários.",

            // Habits
            ["Template_Habits_Sleep"] = "Sono",
            ["Template_Habits_Sleep_Desc"] = "Registe horas e qualidade do sono.",
            ["Template_Habits_Water"] = "Consumo de Água",
            ["Template_Habits_Water_Desc"] = "Conte copos de água por dia.",
            ["Template_Habits_Reading"] = "Leitura",
            ["Template_Habits_Reading_Desc"] = "Registe páginas ou minutos lidos.",
            ["Template_Habits_Meditation"] = "Meditação",
            ["Template_Habits_Meditation_Desc"] = "Registe sessões de meditação.",

            // Finance
            ["Template_Finance_Transaction"] = "Transação",
            ["Template_Finance_Transaction_Desc"] = "Registe receitas e despesas com saldo acumulado.",
            ["Template_Finance_Expense"] = "Despesa",
            ["Template_Finance_Expense_Desc"] = "Registe gastos com categorias.",
            ["Template_Finance_Income"] = "Rendimento",
            ["Template_Finance_Income_Desc"] = "Registe rendimentos de várias fontes.",

            // Fields
            ["Template_Field_Systolic"] = "Sistólica",
            ["Template_Field_Diastolic"] = "Diastólica",
            ["Template_Field_Pulse"] = "Pulso",
            ["Template_Field_Weight"] = "Peso",
            ["Template_Field_Glucose"] = "Glicose",
            ["Template_Field_Meal"] = "Refeição",
            ["Template_Field_WorkoutType"] = "Tipo de Treino",
            ["Template_Field_Duration"] = "Duração",
            ["Template_Field_Calories"] = "Calorias",
            ["Template_Field_Distance"] = "Distância",
            ["Template_Field_Steps"] = "Passos",
            ["Template_Field_Hours"] = "Horas",
            ["Template_Field_Quality"] = "Qualidade",
            ["Template_Field_Glasses"] = "Copos",
            ["Template_Field_Pages"] = "Páginas",
            ["Template_Field_Amount"] = "Montante",
            ["Template_Field_Category"] = "Categoria",
            ["Template_Field_Source"] = "Fonte",
            ["Template_Field_TransactionType"] = "Tipo",

            // Dropdown values
            ["Template_Dropdown_Meal"] = "Jejum,Antes da refeição,Após a refeição,Hora de dormir",
            ["Template_Dropdown_WorkoutType"] = "Força,Cardio,HIIT,Yoga,Natação,Ciclismo,Outro",
            ["Template_Dropdown_Quality"] = "Fraco,Razoável,Bom,Excelente",
            ["Template_Dropdown_TransactionType"] = "Receita,Despesa",
            ["Template_Dropdown_FinanceCategory"] = "Salário,Freelance,Alimentação,Transporte,Habitação,Saúde,Entretenimento,Compras,Investimento,Presente,Outro",
            ["Template_Dropdown_ExpenseCategory"] = "Alimentação,Transporte,Habitação,Saúde,Entretenimento,Compras,Outro",
            ["Template_Dropdown_IncomeSource"] = "Salário,Freelance,Investimento,Presente,Outro",

            // Analytics Rule Labels
            ["Template_Rule_BalanceByCategory"] = "Saldo por Categoria",
            ["Template_Rule_TotalByType"] = "Total por Tipo",
            ["Template_Rule_SpendingByCategory"] = "Gastos por Categoria",
            ["Template_Rule_IncomeByCategory"] = "Rendimento por Categoria",
            ["Template_Rule_IncomeBySource"] = "Rendimento por Fonte",
        },
        ["es"] = new(StringComparer.OrdinalIgnoreCase)
        {
            // Tags
            ["Template_Tag_Health"] = "Salud",
            ["Template_Tag_Vitals"] = "Signos Vitales",
            ["Template_Tag_Fitness"] = "Fitness",
            ["Template_Tag_Exercise"] = "Ejercicio",
            ["Template_Tag_Habits"] = "Hábitos",
            ["Template_Tag_Wellness"] = "Bienestar",
            ["Template_Tag_Finance"] = "Finanzas",

            // Health
            ["Template_Health_BloodPressure"] = "Presión Arterial",
            ["Template_Health_BloodPressure_Desc"] = "Registra sistólica, diastólica y pulso.",
            ["Template_Health_Weight"] = "Peso",
            ["Template_Health_Weight_Desc"] = "Sigue tu peso corporal a lo largo del tiempo.",
            ["Template_Health_BloodGlucose"] = "Glucosa en Sangre",
            ["Template_Health_BloodGlucose_Desc"] = "Monitorea los niveles de azúcar en sangre.",

            // Fitness
            ["Template_Fitness_Workout"] = "Entrenamiento",
            ["Template_Fitness_Workout_Desc"] = "Registra tipo de ejercicio, duración y calorías.",
            ["Template_Fitness_Running"] = "Carrera",
            ["Template_Fitness_Running_Desc"] = "Registra distancia y tiempo.",
            ["Template_Fitness_Steps"] = "Pasos",
            ["Template_Fitness_Steps_Desc"] = "Cuenta tus pasos diarios.",

            // Habits
            ["Template_Habits_Sleep"] = "Sueño",
            ["Template_Habits_Sleep_Desc"] = "Registra horas y calidad del sueño.",
            ["Template_Habits_Water"] = "Consumo de Agua",
            ["Template_Habits_Water_Desc"] = "Cuenta vasos de agua por día.",
            ["Template_Habits_Reading"] = "Lectura",
            ["Template_Habits_Reading_Desc"] = "Registra páginas o minutos leídos.",
            ["Template_Habits_Meditation"] = "Meditación",
            ["Template_Habits_Meditation_Desc"] = "Registra sesiones de meditación.",

            // Finance
            ["Template_Finance_Transaction"] = "Transacción",
            ["Template_Finance_Transaction_Desc"] = "Registra ingresos y gastos con saldo acumulado.",
            ["Template_Finance_Expense"] = "Gasto",
            ["Template_Finance_Expense_Desc"] = "Registra gastos con categorías.",
            ["Template_Finance_Income"] = "Ingreso",
            ["Template_Finance_Income_Desc"] = "Registra ingresos de varias fuentes.",

            // Fields
            ["Template_Field_Systolic"] = "Sistólica",
            ["Template_Field_Diastolic"] = "Diastólica",
            ["Template_Field_Pulse"] = "Pulso",
            ["Template_Field_Weight"] = "Peso",
            ["Template_Field_Glucose"] = "Glucosa",
            ["Template_Field_Meal"] = "Comida",
            ["Template_Field_WorkoutType"] = "Tipo de Ejercicio",
            ["Template_Field_Duration"] = "Duración",
            ["Template_Field_Calories"] = "Calorías",
            ["Template_Field_Distance"] = "Distancia",
            ["Template_Field_Steps"] = "Pasos",
            ["Template_Field_Hours"] = "Horas",
            ["Template_Field_Quality"] = "Calidad",
            ["Template_Field_Glasses"] = "Vasos",
            ["Template_Field_Pages"] = "Páginas",
            ["Template_Field_Amount"] = "Monto",
            ["Template_Field_Category"] = "Categoría",
            ["Template_Field_Source"] = "Fuente",
            ["Template_Field_TransactionType"] = "Tipo",

            // Dropdown values
            ["Template_Dropdown_Meal"] = "Ayuno,Antes de comer,Después de comer,Hora de dormir",
            ["Template_Dropdown_WorkoutType"] = "Fuerza,Cardio,HIIT,Yoga,Natación,Ciclismo,Otro",
            ["Template_Dropdown_Quality"] = "Malo,Regular,Bueno,Excelente",
            ["Template_Dropdown_TransactionType"] = "Ingreso,Gasto",
            ["Template_Dropdown_FinanceCategory"] = "Salario,Freelance,Alimentación,Transporte,Vivienda,Salud,Entretenimiento,Compras,Inversión,Regalo,Otro",
            ["Template_Dropdown_ExpenseCategory"] = "Alimentación,Transporte,Vivienda,Salud,Entretenimiento,Compras,Otro",
            ["Template_Dropdown_IncomeSource"] = "Salario,Freelance,Inversión,Regalo,Otro",

            // Analytics Rule Labels
            ["Template_Rule_BalanceByCategory"] = "Saldo por Categoría",
            ["Template_Rule_TotalByType"] = "Total por Tipo",
            ["Template_Rule_SpendingByCategory"] = "Gastos por Categoría",
            ["Template_Rule_IncomeByCategory"] = "Ingresos por Categoría",
            ["Template_Rule_IncomeBySource"] = "Ingresos por Fuente",
        }
    };

    /// <summary>
    /// Resolves a localization key to its translated value for the given language.
    /// Falls back to the provided English default if no translation is found.
    /// </summary>
    public static string Resolve(string? key, string englishDefault, string? language)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(language))
            return englishDefault;

        var lang = language.Length > 2 ? language[..2] : language;

        if (Translations.TryGetValue(lang, out var dict) && dict.TryGetValue(key, out var translated))
            return translated;

        return englishDefault;
    }
}

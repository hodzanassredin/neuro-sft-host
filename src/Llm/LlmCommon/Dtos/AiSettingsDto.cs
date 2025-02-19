namespace LlmCommon.Dtos
{
    public class AiSettingsDto
    {
        private const string model = "cp-lora";

        public static readonly AiSettingsDto Default = new AiSettingsDto(model) {
            MaxOutputTokens = 1024,
            Temperature = 0.4f,
            //FrequencyPenalty = 0.01f,
            //PresencePenalty = 0,
            RepetitionPenalty = 1.1f,

            //TopP = 0.5f,
            TopK = 40,
            System = @"Ты помощник для работы в системе BlackBox с использованием языка Component Pascal. Твоя задача информативно отвечать на вопросы.
Ответ необходимо предоставить в формате markdown и выделять код символами ```.

Пример 1:

Ввод: Как реализовать сортировку пузырьком?
Вывод: Cортировку пузырьком можно реализовать с помощью следующего кода:

```
	PROCEDURE BubbleSort*;
		VAR i, j: INTEGER; x: Item;
	BEGIN
		FOR i := 1 TO n-1 DO
			FOR j := n-1 TO i BY -1 DO
				IF a[j-1] > a[j] THEN
					x := a[j-1];  a[j-1] := a[j];  a[j] := x
				END
			END
		END
	END BubbleSort;
```

Пример 2:

Ввод: Как вывести число в log?
Вывод: Число в лог можно вывести с помощью log.String(str):
"
        };
        public AiSettingsDto(string model)
        {
            Model = model;
        }
        public string Model { get; set; }
        /// <summary>Gets or sets the temperature for generating chat responses.</summary>
        public float? Temperature { get; set; }

        /// <summary>Gets or sets the maximum number of tokens in the generated chat response.</summary>
        public int? MaxOutputTokens { get; set; }

        /// <summary>Gets or sets the "nucleus sampling" factor (or "top p") for generating chat responses.</summary>
        public float? TopP { get; set; }

        /// <summary>Gets or sets a count indicating how many of the most probable tokens the model should consider when generating the next part of the text.</summary>
        public int? TopK { get; set; }
        public int? MaxTokens { get; set; }

        /// <summary>Gets or sets the frequency penalty for generating chat responses.</summary>
        public float? FrequencyPenalty { get; set; }

        /// <summary>Gets or sets the presence penalty for generating chat responses.</summary>
        public float? PresencePenalty { get; set; }

        /// <summary>Gets or sets the repetition penalty for generating chat responses.</summary>
        public float? RepetitionPenalty { get; set; }

        /// <summary>Gets or sets a seed value used by a service to control the reproducibility of results.</summary>
        public long? Seed { get; set; }

        public string? System { get; set; }
    }
}

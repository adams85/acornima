namespace Acornima;

public enum EcmaVersion
{
    Unknown,

    ES3 = 3,
    ES5 = 5,
    ES6 = 6,
    ES7 = 7,
    ES8 = 8,
    ES9 = 9,
    ES10 = 10,
    ES11 = 11,
    ES12 = 12,
    ES13 = 13,
    ES14 = 14,

    ES2015 = ES6,
    ES2016 = ES7,
    ES2017 = ES8,
    ES2018 = ES9,
    ES2019 = ES10,
    ES2020 = ES11,
    ES2021 = ES12,
    ES2022 = ES13,
    ES2023 = ES14,
    ES2024 = ES15,
    ES2025 = ES16,
    ES2026 = ES17,

    /// <summary>
    /// The latest version which is fully supported (<see cref="ES2023"/>).
    /// </summary>
    Latest = ES2023,
}

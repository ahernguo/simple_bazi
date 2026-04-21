using Fluxor;

namespace BaZi.Store {
    public static class BaZiReducers {
        [ReducerMethod]
        public static BaZiState OnSetBirthDate(BaZiState state, SetBirthDateAction action) =>
            new(
                isLoading: state.IsLoading,
                isCalculated: state.IsCalculated,
                error: state.Error,
                birthDate: action.BirthDate,
                gender: state.Gender,
                info: state.Info
            );

        [ReducerMethod]
        public static BaZiState OnSetGender(BaZiState state, SetGenderAction action) =>
            new(
                isLoading: state.IsLoading,
                isCalculated: state.IsCalculated,
                error: state.Error,
                birthDate: state.BirthDate,
                gender: action.Gender,
                info: state.Info
            );

        [ReducerMethod]
        public static BaZiState OnCalculate(BaZiState state, CalculateBaZiAction action) =>
            new(
                isLoading: true,
                isCalculated: false,
                error: null,
                birthDate: action.BirthDate,
                gender: action.Gender,
                info: null
            );

        [ReducerMethod]
        public static BaZiState OnCalculateSuccess(BaZiState state, CalculateBaZiSuccessAction action) =>
            new(
                isLoading: false,
                isCalculated: true,
                error: null,
                birthDate: state.BirthDate,
                gender: state.Gender,
                info: action.Info
            );

        [ReducerMethod]
        public static BaZiState OnCalculateFailure(BaZiState state, CalculateBaZiFailureAction action) =>
            new(
                isLoading: false,
                isCalculated: false,
                error: action.Error,
                birthDate: state.BirthDate,
                gender: state.Gender,
                info: null
            );
    }
}

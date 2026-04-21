namespace BaZi.Store {
    public class SetBirthDateAction {
        public DateTime BirthDate { get; }
        public SetBirthDateAction(DateTime birthDate) => BirthDate = birthDate;
    }

    public class SetGenderAction {
        public int Gender { get; }
        public SetGenderAction(int gender) => Gender = gender;
    }

    public class CalculateBaZiAction {
        public DateTime BirthDate { get; }
        public int Gender { get; }

        public CalculateBaZiAction(DateTime birthDate, int gender) {
            BirthDate = birthDate;
            Gender = gender;
        }
    }

    public class CalculateBaZiSuccessAction {
        public Models.BaZiInfo Info { get; }
        public CalculateBaZiSuccessAction(Models.BaZiInfo info) => Info = info;
    }

    public class CalculateBaZiFailureAction {
        public string Error { get; }
        public CalculateBaZiFailureAction(string error) => Error = error;
    }
}

namespace MLModelApi.MLModel
{
    public class ModelInput
    {
        public float Day { get; set; }
        public float Month { get; set; }
        public float Year { get; set; }
        public float Weekday { get; set; }
        public float IsHoliday { get; set; }
        public float IsWeekend { get; set; }
        public float Temp { get; set; }
        public float Humidity { get; set; }
        public float StoreId { get; set; }
        public string Region { get; set; }
        public float Promotion { get; set; }
        public float Discount { get; set; }
    }
}

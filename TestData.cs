namespace mParticleAPI;

public class TestData
{
    public DateTime Date { get; set; }

    public int RequestsSent { get; set; }

    public required string Name { get; set; }
}

public class TestDataResponse
{
    public bool Successful { get; set; }
}

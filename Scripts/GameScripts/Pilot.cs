public class Pilot{
	public int Pilotting = 5;
	public int Gunnery = 4;
	public int Initiative = 3;

	public Pilot(){
		System.Random rnd = new System.Random();
		Initiative = rnd.Next(Globals.MinPilotInitiative, Globals.MaxPilotInitiative + 1);
	}
}
//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {
				public event EventHandler<IVector>? NewPositionNotification;
    public IVector Velocity {  get; set; }
    public IVector Position { get; set; }
    public double Mass { get; }
    public double Radius { get; }

    // Zmienna odpowiedzialna za zatrzymanie pętli, a co za tym idzie wątków
    private bool stopped = false;
    private readonly object positionLock = new object();

				internal Ball(Vector initialPosition, Vector initialVelocity, double mass, double radius)
    {
      Position = initialPosition;
      Velocity = initialVelocity;
      Mass = mass;
      Radius = radius;

      Task.Run(Move);
    }


    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

    private async Task Move()
    {
      // Wykonywanie zadania do momentu zasygnalizowania chęci usunięcia obiektu
      while(!stopped)
      {

        // Zapewnienie, że operacja zmiany pozycji wykona się w całości
        lock (positionLock)
        {
          Position = new Vector(Position.x + Velocity.x, Position.y + Velocity.y);
        }

        // Zakomunikowanie warstwie wyżej o zmianie pozycji kuli
								RaiseNewPositionChangeNotification();
								await Task.Delay(16);
						}
    }

    public void Dispose()
    {
      stopped = true;
    }
  }
}
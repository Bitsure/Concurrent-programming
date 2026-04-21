//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
				//private bool disposedValue;
				private bool Disposed = false;

				private Random RandomGenerator = new();
				private List<Ball> BallsList = [];

				public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
    {
      // Sprawdzanie, czy obiekt nie został już usunięty i czy przekazano `upperLayerHandler`
      if (Disposed)
        throw new ObjectDisposedException(nameof(DataImplementation));
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(upperLayerHandler));

      // Tworzenie podanej liczby kul
      Random random = new Random();
      for (int i = 0; i < numberOfBalls; i++)
      {
        double radius = 10;
        double mass = RandomGenerator.Next(10, 30);

        Vector startingPosition = new(RandomGenerator.Next(50, 350), RandomGenerator.Next(50, 350));
								Vector startingVelocity = new((RandomGenerator.NextDouble() - 0.5) * 5, (RandomGenerator.NextDouble() - 0.5) * 5);

								Ball newBall = new(startingPosition, startingVelocity, mass, radius);
        upperLayerHandler(startingPosition, newBall);
        BallsList.Add(newBall);
      }
    }

				protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          BallsList.Clear();
        }
        Disposed = true;
      }
      else
        throw new ObjectDisposedException(nameof(DataImplementation));
    }

    public override void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
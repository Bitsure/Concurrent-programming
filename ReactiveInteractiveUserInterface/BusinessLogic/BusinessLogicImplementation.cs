//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;
using DataBall = TP.ConcurrentProgramming.Data.IBall;
using DataVector = TP.ConcurrentProgramming.Data.Vector;

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
  {
				private bool Disposed = false;

				private readonly UnderneathLayerAPI layerBellow;

    // Stała służąca do synchronizacji
    private readonly object collisionLock = new object();

    // Lista kul znajdujących się na planszy
    private readonly List<DataBall> activeBalls = new();

				public BusinessLogicImplementation() : this(null)
    { }

    internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
    {
      layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
    }

    public override void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

      layerBellow.Dispose();
      Disposed = true;
    }

    public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
    {
      // Sprawdzanie, czy obiekt nie został wcześniej usunięty i czy przekazano `upperLayerHandler`
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(upperLayerHandler));

      // Poproszenie warstwy danych o utworzenie kul i rozpoczęcie symulacji
      layerBellow.Start(numberOfBalls, (startingPosition, databall) => {
        
        // Zapewnienie dostępu do listy kul tylko dla jednego wątku w danym czasie
        lock (collisionLock)
        {
          activeBalls.Add(databall);
        }

        // Odbieranie nowej pozycji kuli z warstwy niżej i sprawdzanie jej względem logiki
        databall.NewPositionNotification += (sender, vector) =>
        {
          CheckCollisions((DataBall)sender!);
        };

        // Opakowanie danych odnośnie kuli i przesłanie ich do warstwy widoku
        upperLayerHandler(new Position(startingPosition.x, startingPosition.y), new Ball(databall));
      });
    }

    private void CheckCollisions(DataBall movingBall)
    {
      lock (collisionLock)
      {
        // Wymiary obszaru, w którym dochodzi do symulacji
        double areaWidth = 386.0;
        double areaHeight = 406.0;

        // Sprawdzanie, czy kula zbliża się do krawędzi planszy X
        if (movingBall.Position.x <= 0 || movingBall.Position.x + movingBall.Radius >= areaWidth)
        {
          movingBall.Velocity = new DataVector(-movingBall.Velocity.x , movingBall.Velocity.y);
        }

								// Sprawdzanie, czy kula zbliża się do krawędzi planszy Y
								if (movingBall.Position.y <= 0 || movingBall.Position.y + movingBall.Radius >= areaHeight)
								{
										movingBall.Velocity = new DataVector(movingBall.Velocity.x, -movingBall.Velocity.y);
								}

        // Sprawdzanie zderzeń sprężystych między poszczególnymi kulami
        foreach (DataBall otherBall in activeBalls)
        {
          // Pomijanie sprawdzania samego siebie
          if (otherBall == movingBall) continue;

          // Obliczanie odległości od poszczególnych kul
          double dx = otherBall.Position.x - movingBall.Position.x;
          double dy = otherBall.Position.y - movingBall.Position.y;
          double distance = Math.Sqrt(dx * dx + dy * dy);

          // Jeśli odległość jest mniejsza niż suma ich promieni, to się zderzyły
          if (distance < movingBall.Radius + otherBall.Radius)
          {
												double overlap = (movingBall.Radius + otherBall.Radius) - distance;

            double nx = dx / distance;
            double ny = dy / distance;

            double relativeVelX = otherBall.Velocity.x - movingBall.Velocity.x;
            double relativeVelY = otherBall.Velocity.y - movingBall.Velocity.y;
            double relation = relativeVelX * nx + relativeVelY * ny;
            if (relation >= 0) continue;

            // Zapobieganie ruchów wahadłowych dwóch kul przy niektórych kolizjach
            movingBall.Position = new DataVector(
              movingBall.Position.x - nx * (overlap / 2),
              movingBall.Position.y - ny * (overlap / 2)
            );

												otherBall.Position = new DataVector(
														otherBall.Position.x - nx * (overlap / 2),
														otherBall.Position.y - ny * (overlap / 2)
            );

												double m1 = movingBall.Mass;
            double m2 = otherBall.Mass;

            // Obliczanie prędkości obecnej kuli
            double newVelocityX1 = (movingBall.Velocity.x * (m1 - m2) + (2 * m2 * otherBall.Velocity.x)) / (m1 + m2);
            double newVelocityY1 = (movingBall.Velocity.y * (m1 - m2) + (2 * m2 * otherBall.Velocity.y)) / (m1 + m2);

            // Obliczanie prędkości kuli, z którą się zderzyła
            double newVelocityX2 = (otherBall.Velocity.x * (m2 - m1) + (2 * m1 * movingBall.Velocity.x)) / (m1 + m2);
            double newVelocityY2 = (otherBall.Velocity.y * (m2 - m1) + (2 * m1 * movingBall.Velocity.y)) / (m1 + m2);

            // Ustawianie prędkości dla tych dwóch kul
            movingBall.Velocity = new DataVector(newVelocityX1, newVelocityY1);
            otherBall.Velocity = new DataVector(newVelocityX2, newVelocityY2);
          }
        }
						}
    }

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

  }
}
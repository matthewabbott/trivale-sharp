// src/Game/Services/GameEventCoordinator.cs
using System;
using Godot;

namespace Trivale.Game.Services
{
    public interface IGameEventCoordinator
    {
        void Initialize(Node signalSource);
        void EmitGameStateChanged();
        void EmitTrickCompleted(int winner);
        void EmitGameOver(int winner);
        void HandleGameStateChanged();
        void HandleTrickCompleted(int winner);
        void HandleGameOver(int winner);
        void Cleanup();
    }

    public class GameEventCoordinator : IGameEventCoordinator
    {
        private Node _signalSource;
        private bool _initialized;

        public void Initialize(Node signalSource)
        {
            _signalSource = signalSource;
            _initialized = true;
        }

        public void EmitGameStateChanged()
        {
            if (!_initialized) return;
            _signalSource.EmitSignal(GameState.SignalName.GameStateChanged);
        }

        public void EmitTrickCompleted(int winner)
        {
            if (!_initialized) return;
            _signalSource.EmitSignal(GameState.SignalName.TrickCompleted, winner);
        }

        public void EmitGameOver(int winner)
        {
            if (!_initialized) return;
            _signalSource.EmitSignal(GameState.SignalName.GameOver, winner);
        }

        // These methods are called by game components
        public void HandleGameStateChanged()
        {
            EmitGameStateChanged();
        }

        public void HandleTrickCompleted(int winner)
        {
            EmitTrickCompleted(winner);
        }

        public void HandleGameOver(int winner)
        {
            EmitGameOver(winner);
        }

        public void Cleanup()
        {
            _signalSource = null;
            _initialized = false;
        }
    }
}
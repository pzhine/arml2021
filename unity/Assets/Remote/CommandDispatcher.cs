using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using WorldAsSupport.WorldAPI;

namespace WorldAsSupport.Remote {
  public class CommandDispatcher {
    public delegate void HandleLoadWorldDoc(WorldDoc worldDoc);
    public event HandleLoadWorldDoc OnLoadWorldDoc;
    
    public delegate void HandleStartGame();
    public event HandleStartGame OnStartGame;
    
    public delegate void HandleToggleProjector(bool isOn);
    public event HandleToggleProjector OnToggleProjector;
    
    public delegate void HandleToggleFlashlight(bool isOn);
    public event HandleToggleFlashlight OnToggleFlashlight;

    public delegate void HandleSendTest(string message);
    public event HandleSendTest OnSendTest;

    private Sender m_Sender;

    public CommandDispatcher(Sender sender) {
      this.m_Sender = sender;
    }

    public void LoadWorldDoc(
      WorldDocData worldDocData,
      WorldVersionData worldVersionData,
      byte[] worldMap, 
      FakeARWorldMapData fakeARWorldMapData = null
    ) {
      LoadWorldDocData payloadData = new LoadWorldDocData();
      
      byte[] mapBytes;
      if (worldMap != null) {
        mapBytes = worldMap;
      } else {
        mapBytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(fakeARWorldMapData));
      }

      payloadData.worldDocData = worldDocData;
      payloadData.worldVersionData = worldVersionData;
      CommandData commandData = new CommandData();
      commandData.payload = JsonUtility.ToJson(payloadData);
      commandData.type = "LoadWorldDoc";
      
      m_Sender.SendCommand(commandData, mapBytes);
    }

    public void StartGame() {
      CommandData commandData = new CommandData();
      commandData.type = "StartGame";
      m_Sender.SendCommand(commandData);
    }

    public void ToggleProjector(bool isOn) {
      CommandData commandData = new CommandData();
      commandData.type = "ToggleProjector";
      ToggleProjectorData payloadData = new ToggleProjectorData();
      payloadData.isOn = isOn;
      commandData.payload = JsonUtility.ToJson(payloadData);
      m_Sender.SendCommand(commandData);
    }
    
    public void ToggleFlashlight(bool isOn) {
      CommandData commandData = new CommandData();
      commandData.type = "ToggleFlashlight";
      ToggleFlashlightData payloadData = new ToggleFlashlightData();
      payloadData.isOn = isOn;
      commandData.payload = JsonUtility.ToJson(payloadData);
      m_Sender.SendCommand(commandData);
    }

    public void SendTest(string message) {
      TestData payloadData = new TestData();
      payloadData.message = message;
      CommandData commandData = new CommandData();
      commandData.payload = JsonUtility.ToJson(payloadData);
      commandData.type = "Test";
      m_Sender.SendCommand(commandData);
    }

    public void HandleIncomingCommand(CommandData commandData, byte[] data) {
      switch (commandData.type) {
        case "Test": {
          if (this.OnSendTest != null) {
            TestData payloadData = JsonUtility.FromJson<TestData>(
              commandData.payload
            );
            this.OnSendTest(payloadData.message);
          }
          break;
        }
        case "StartGame": {
          if (this.OnStartGame != null) {
            this.OnStartGame();
          }
          break;
        }
        case "ToggleProjector": {
          if (this.OnToggleProjector != null) {
            ToggleProjectorData payloadData = JsonUtility.FromJson<ToggleProjectorData>(
              commandData.payload
            );
            this.OnToggleProjector(payloadData.isOn);
          }
          break;
        }
        case "ToggleFlashlight": {
          if (this.OnToggleFlashlight != null) {
            ToggleFlashlightData payloadData = JsonUtility.FromJson<ToggleFlashlightData>(
              commandData.payload
            );
            this.OnToggleFlashlight(payloadData.isOn);
          }
          break;
        }
        case "LoadWorldDoc": {
          if (this.OnLoadWorldDoc != null) {
            LoadWorldDocData payloadData = JsonUtility.FromJson<LoadWorldDocData>(
              commandData.payload
            );
            byte[] worldMapBytes = null;
            FakeARWorldMapData fakeARWorldMapData = null;
          #if !UNITY_EDITOR && UNITY_IOS
            worldMapBytes = data;
          #else
            fakeARWorldMapData = JsonUtility.FromJson<FakeARWorldMapData>(
              Encoding.UTF8.GetString(data)
            );
          #endif
            WorldDoc worldDoc = new WorldDoc(
              payloadData.worldDocData,
              payloadData.worldVersionData, 
              worldMapBytes, 
              fakeARWorldMapData
            );
            this.OnLoadWorldDoc(worldDoc);
          }
          break;
        }
      }
    }
  }
  [Serializable]
  public class CommandData {
    public string type;
    public string payload; // serialized payload
  }
  [Serializable]
  public class LoadWorldDocData {
    public WorldDocData worldDocData;
    public WorldVersionData worldVersionData;
  }
  [Serializable]
  public class StartGameData {}
  [Serializable]
  public class ToggleProjectorData {
    public bool isOn;
  }
  [Serializable]
  public class ToggleFlashlightData {
    public bool isOn;
  }
  [Serializable]
  public class TestData {
    public string message;
  }
  
}
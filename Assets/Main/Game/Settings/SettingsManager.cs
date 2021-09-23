using MPGUI;
using Serialization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MPCore
{
	public class SettingsManager : MonoBehaviour
	{
		const string F2 = "F2";
		const string NONE = "None";

		SettingsModel _settingsModel;
		KeyModel _keyModel;
		GraphicsModel _graphicsModel;
		PlaySettingsModel _playModel;

		void Awake()
		{
			_settingsModel = Models.GetModel<SettingsModel>();
			_keyModel = Models.GetModel<KeyModel>();
			_graphicsModel = Models.GetModel<GraphicsModel>();
			_playModel = Models.GetModel<PlaySettingsModel>();

			_settingsModel.Save.AddListener(Save);
			_settingsModel.Load.AddListener(Load);
		}

		void Start()
		{
			_settingsModel.Load?.Invoke();
		}

		void OnDestroy()
		{
			Models.RemoveModel<SettingsModel>();
			_settingsModel.Save.RemoveListener(Save);
			_settingsModel.Load.RemoveListener(Load);
		}

		void Load()
		{
			if (_settingsModel.data != null)
				Deserialize(_settingsModel.data);
		}

		void Save()
		{
			Serialize();
			XMLSerialization.Save(_settingsModel.data, "settings");
		}

		void Serialize()
		{
			SettingsData data = new SettingsData();

			// Key Model ------------------------------------------------------
			// Keys
			foreach (KeyBind kb in _keyModel.keys)
				data.SetKey(kb.name, kb.keyCombo);

			data.SetValue(nameof(_keyModel.mouseSensitivity), _keyModel.mouseSensitivity.Value.ToString(F2));
			data.SetValue(nameof(_keyModel.sprintToggleTime), _keyModel.sprintToggleTime.Value.ToString(F2));
			data.SetValue(nameof(_keyModel.walkToggleTime), _keyModel.walkToggleTime.Value.ToString(F2));
			data.SetValue(nameof(_keyModel.crouchToggleTime), _keyModel.crouchToggleTime.Value.ToString(F2));
			data.SetValue(nameof(_keyModel.alwaysRun), _keyModel.alwaysRun.Value.ToString());

			// Settings Model -------------------------------------------------
			// Mixer Volume
			foreach (string name in _settingsModel.mixerFloatParameters)
				if (_settingsModel.mixer.GetFloat(name, out float value))
					data.SetValue(name, value.ToString(F2));

			// GraphicsModel --------------------------------------------------
			data.SetValue(nameof(_graphicsModel.fov), _graphicsModel.fov.Value.ToString(F2));
			data.SetValue(nameof(_graphicsModel.fovFirstPerson), _graphicsModel.fovFirstPerson.Value.ToString(F2));
			data.SetValue(nameof(_graphicsModel.pixelationShader), _graphicsModel.pixelationShader ? _graphicsModel.pixelationShader.name : NONE);
			data.SetValue(nameof(_graphicsModel.bloomEnabled), _graphicsModel.bloomEnabled.Value.ToString());
			data.SetValue(nameof(_graphicsModel.iterations), _graphicsModel.iterations.ToString());
			data.SetValue(nameof(_graphicsModel.filmGrainScale), _graphicsModel.filmGrainScale.ToString(F2));

			// PlayModel ------------------------------------------------------
			data.SetValue(nameof(_playModel.scene), _playModel.scene ? _playModel.scene.name : NONE);
			data.SetValue(nameof(_playModel.botCount), _playModel.botCount.ToString());
			data.SetValue(nameof(_playModel.game), _playModel.game ? _playModel.game.name : NONE);
			data.SetValue(nameof(_playModel.mutators), string.Join(",", _playModel.mutators.Select(mut => mut.name)));

			_settingsModel.OnSerialize?.Invoke(data);
			_settingsModel.data = data;
		}

		void Deserialize(SettingsData data)
		{
			float fValue;
			bool bValue;
			string sValue;
			int iValue;

			// Key Model ------------------------------------------------------
			// Keys
			foreach (KeyBind kb in _keyModel.keys)
				if (data.GetKey(kb.name).Length > 0)
					kb.keyCombo = data.GetKey(kb.name);

			// Fields
			if (float.TryParse(data.GetValue(nameof(_keyModel.mouseSensitivity)), out fValue))
				_keyModel.mouseSensitivity.Value = fValue;
			if (float.TryParse(data.GetValue(nameof(_keyModel.sprintToggleTime)), out fValue))
				_keyModel.sprintToggleTime.Value = fValue;
			if (float.TryParse(data.GetValue(nameof(_keyModel.walkToggleTime)), out fValue))
				_keyModel.walkToggleTime.Value = fValue;
			if (float.TryParse(data.GetValue(nameof(_keyModel.crouchToggleTime)), out fValue))
				_keyModel.crouchToggleTime.Value = fValue;
			if (bool.TryParse(data.GetValue(nameof(_keyModel.alwaysRun)), out bValue))
				_keyModel.alwaysRun.Value = bValue;

			// Settings Model -------------------------------------------------
			// Mixer Volume
			foreach (string name in _settingsModel.mixerFloatParameters)
				if (float.TryParse(data.GetValue(name), out float value))
					_settingsModel.mixer.SetFloat(name, value);

			// Graphics Model -------------------------------------------------
			if (float.TryParse(data.GetValue(nameof(_graphicsModel.fov)), out fValue))
				_graphicsModel.fov.Value = fValue;
			if (float.TryParse(data.GetValue(nameof(_graphicsModel.fovFirstPerson)), out fValue))
				_graphicsModel.fovFirstPerson.Value = fValue;
			if (data.TryGetValue(nameof(_graphicsModel.pixelationShader), out sValue))
				_graphicsModel.pixelationShader = _graphicsModel.pixelationOptions?.Find(mat => (!mat && sValue == NONE) || (mat && mat.name == sValue));
			if (bool.TryParse(data.GetValue(nameof(_graphicsModel.bloomEnabled)), out bValue))
				_graphicsModel.bloomEnabled.Value = bValue;
			if (int.TryParse(data.GetValue(nameof(_graphicsModel.iterations)), out iValue))
				_graphicsModel.iterations = iValue;
			if (float.TryParse(data.GetValue(nameof(_graphicsModel.filmGrainScale)), out fValue))
				_graphicsModel.filmGrainScale = fValue;

			// Play Model -----------------------------------------------------
			Case[] caseList = ResourceLoader.GetResources<Case>();
			Mutator[] mutList = ResourceLoader.GetResources<Mutator>();
			GameController[] gameList = ResourceLoader.GetResources<GameController>();

			if (data.TryGetValue(nameof(_playModel.scene), out sValue) && sValue != NONE)
				_playModel.scene = caseList.FirstOrDefault(cas => cas && cas.name == sValue);
			if (int.TryParse(data.GetValue(nameof(_playModel.botCount)), out iValue))
				_playModel.botCount = iValue;
			if (data.TryGetValue(nameof(_playModel.game), out sValue))
				_playModel.game = gameList.FirstOrDefault(gam => (!gam && sValue == NONE) || (gam && gam.name == sValue));
			if (data.TryGetValue(nameof(_playModel.mutators), out sValue))
				_playModel.mutators = sValue.Split(',')
					.Select(mutName => mutList.FirstOrDefault(mut => mut.name == mutName))
					.Where(mut => mut)
					.ToList();

			_settingsModel.OnDeserialize?.Invoke(data);
		}
	}
}

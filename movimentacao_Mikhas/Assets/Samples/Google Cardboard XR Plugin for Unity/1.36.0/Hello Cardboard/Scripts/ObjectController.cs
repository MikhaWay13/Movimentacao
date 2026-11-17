//-----------------------------------------------------------------------
// <copyright file="ObjectController.cs" company="Google LLC">
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

using UnityEngine;

/// <summary>
/// Controls target objects behaviour.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ObjectController : MonoBehaviour
{
    /// <summary>
    /// The material to use when this object is inactive (not being gazed at).
    /// </summary>
    public Material InactiveMaterial;

    /// <summary>
    /// The material to use when this object is active (gazed at).
    /// </summary>
    public Material GazedAtMaterial;

    /// <summary>
    /// Arquivo de áudio que será reproduzido ao clicar no objeto.
    /// </summary>
    public AudioClip Musica;

    private Renderer _myRenderer;
    private AudioSource _audioSource;

    /// <summary>
    /// Start is called before the first frame update.
    /// </summary>
    public void Start()
    {
        _myRenderer = GetComponent<Renderer>();
        _audioSource = GetComponent<AudioSource>();

        // Evita que a música comece tocando sozinha assim que o jogo iniciar
        _audioSource.playOnAwake = false;
        _audioSource.clip = Musica;

        SetMaterial(false);
    }

    /// <summary>
    /// Toca a música quando acionado pelo clique/toque.
    /// </summary>
    public void PlayMusic()
    {
        if (Musica == null)
        {
            Debug.LogWarning("Nenhuma música foi atribuída no Inspector do objeto: " + gameObject.name);
            return;
        }

        // Se já estiver tocando, reinicia a música do começo.
        // (Se preferir que o segundo clique pause a música, troque o bloco abaixo por: if (_audioSource.isPlaying) { _audioSource.Pause(); return; })
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        _audioSource.Play();
    }

    /// <summary>
    /// This method is called by the Main Camera when it starts gazing at this GameObject.
    /// </summary>
    public void OnPointerEnter()
    {
        SetMaterial(true);
    }

    /// <summary>
    /// This method is called by the Main Camera when it stops gazing at this GameObject.
    /// </summary>
    public void OnPointerExit()
    {
        SetMaterial(false);
    }

    /// <summary>
    /// This method is called by the Main Camera when it is gazing at this GameObject and the screen
    /// is touched.
    /// </summary>
    public void OnPointerClick()
    {
        PlayMusic();
    }

    /// <summary>
    /// Sets this instance's material according to gazedAt status.
    /// </summary>
    private void SetMaterial(bool gazedAt)
    {
        if (InactiveMaterial != null && GazedAtMaterial != null)
        {
            _myRenderer.material = gazedAt ? GazedAtMaterial : InactiveMaterial;
        }
    }
}

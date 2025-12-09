# Unity Project for Languagelearning
The Project source code for my Bachelors Thesis on IVA Based Language Learning.

#### About
The goal of this thesis was to explore the use of a Bilingual Intelligent Virtual Agent, capable of conversing in both French and German in a virtual Café Setting.  

## Requirements
- Unity Version: 2022.3 LTS
- VR Headset: Meta Quest 2 / Quest 3
- OpenAI API Key
- Backend server running local python models (Languagelearning repository)

## Setup
#### Backend (Speech & TTS Server)

This project requires a local backend server for speech recognition and text-to-speech.
Follow the instructions in my Languagelearning repository.

Make sure the Server is running before starting the Unity project.

#### Unity Configuration

Open the Unity project using Unity 2022.3 LTS.

Open the scene:
```
Assets/Scenes/ParisScene.unity
```
In the Project window:

- Right-click → Create → Config

- Use Assets/Scripts/Config/ConfigTEMPLATE.cs File as a reference to build your own config. (Remove the word TEMPLATE)

In your new Config Asset, set:

- Your OpenAI API Key

- The IP address of the machine running whisper_server.py

#### Service Binding

Assign the created Config Asset to the following GameObjects in the Hierarchy:

Path (in Hierarchy):
```
Scripts/Model/Network
```

Assign it to:
- ChatGPTService
- SynthesisService
- TranscriptionService

Use the Inspector to link the asset.
## Have Fun!
Build and Run on your Meta headset.

#### Controls
- Hold down the A Button / Primary Button for Push-to-Talk. 
- Use Grab Button to Interact with spawned Objects 

#### Prompt Experimenting
If you want to change the system prompt, go to Assets/Scripts/Controller/DialogueController.cs

#### Changing Languages
If you want to change the target language of the IVA
- go to Assets/Scripts/Model/Network/ChatGPTService.cs and change "fr" to whatever you prefer (in responseFormat String)
- Develop your own System Prompt, designed for the target language (DialogueController.cs)
- Download a Voice Model from Piper-TTS Repo for the language, and replace fr_FR-tom-medium with your model in whisper_server.py

Enjoy!


## License
This project is licensed under the MIT License – see the LICENSE file for details.

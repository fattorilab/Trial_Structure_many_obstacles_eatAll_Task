import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

my_path = Path(r "C:/Users/fatto/Desktop/Funzionanti con saver/")

data = pd.read_csv(my_path.joinpath("/Forest_TrialStructure/Assets/StreamingAssets/data/exampledata.csv", sep =';', skipinitialspace=True)
#documentation:
    #Time: that has passed since the experiment has been started
    #trial: a block that ends when a reward was given
    #trial_with_repeats: a block that ends when a target was activated
    #*_active: was the row active?
    #interval: randomized time in seconds it took for the fruit to turn red
    #player_*_arduino: arduino axis input (example data was made with a keyboard, so no arduino input)
    #player_*: Player position and orientation in space
    
supplement = pd.read_csv((my_path.joinpath("/Forest_TrialStructure/Assets/StreamingAssets/data/exampledata.csv", sep =';', skipinitialspace=True)
#documentation:
    #name, x, y, orientation: identifier and position in space (y matches player_z)
    #info:  type of data that was saved
    #TimeEntry: time of creation
    #TimeExit: time of deletion (-1 if no deletion; so be careful with "<")
#Different info values:
    #Obstacle: The trees
    #Seed: the seed for the randomness that was used
    #Position: Positions of the possible target objects
    #FruitTrigger: info from a correct target that was activated
    #WrongFruitTrigger: info from a target that was activated
    #Trigger: Triggers for future synchronization:
        #1 space key (start of a proper data collection)
        #2 correct target was activated
        #3 incorrect target was activated
        #4 a target was reached (turned grey)
        #5 target(s) turned green (looking)
        #6 target(s) turned red (moving)
       
targets = supplement[supplement["info"] == "Position"]
data = data[data["trial_with_repeats"] != data["trial_with_repeats"].iloc[-1]] #last trial remains unfinished
supplement = supplement[supplement["TimeEntry"] < data["Time"].iloc[-1]] #last trial remains unfinished


def get_trial_data(trial_nr): #using trial_with_repeats
    trial_data = data[data["trial_with_repeats"] == trial_nr]
    trial_supplement = supplement[(supplement["TimeEntry"] >= trial_data["Time"].iloc[0]) & (supplement["TimeEntry"] <= trial_data["Time"].iloc[-1])]
    correct_target = targets[targets["Name"] == trial_data["correct_target"].iloc[1]]
    return trial_data, trial_supplement, correct_target


#Let's see a trial
def show_trial(trial_nr):
    trial_data, trial_supplement, correct_target = get_trial_data(trial_nr)
    right_activations = trial_supplement[trial_supplement["info"] == "FruitTrigger"]
    wrong_activations = trial_supplement[trial_supplement["info"] == "WrongFruitTrigger"]
    
    #show path
    plt.plot(trial_data["player_x"], trial_data["player_z"]) #show path
    
    #show objects
    if (trial_data["close_active"].iloc[1] == True):
        plt.plot(targets["x"].iloc[:3], targets["y"].iloc[:3], 'o', color='black')
    if (trial_data["middle_active"].iloc[1] == True):
        plt.plot(targets["x"].iloc[3:6], targets["y"].iloc[3:6], 'o', color='black')
    if (trial_data["far_active"].iloc[1] == True):
        plt.plot(targets["x"].iloc[6:], targets["y"].iloc[6:], 'o', color='black')
        
    #show correct target
    if (len(right_activations) > 0):
        plt.plot(right_activations["x"], right_activations["y"], 'o', color='lime')
    else:
        plt.plot(correct_target["x"], correct_target["y"], 'o', color='cyan')
    if (len(wrong_activations) > 0):
        plt.plot(wrong_activations["x"], wrong_activations["y"], 'o', color='red')
    
    #make plot
    plt.xlim(-7, 7.01)
    ax = plt.gca()
    ax.set_aspect('equal', adjustable='box')
    plt.show()


show_trial(5)


#About a trial
trial_length = data.groupby('trial')['Time'].apply(lambda x: (x.iloc[-1] - x.iloc[0])/1000)
print("A typical trial took " + str(np.round(trial_length.mean(), decimals=2)) + " seconds")

print("The correct target was selected " +  str(np.round(data["trial"].iloc[-1]/data["trial_with_repeats"].iloc[-1]*100, decimals=2)) + "% of the time")

targets_turned_red = supplement[(supplement["info"] == "Trigger") & (supplement["x"] == 6)]
reaction_to_target_turning_red = []
for time in targets_turned_red["TimeEntry"]:
    after_time = data[data["Time"] > time]
    first_nonzero_time = after_time.loc[after_time["player_z"] != 0, "Time"].iloc[0]
    reaction_to_target_turning_red.append(first_nonzero_time)
print("Once movement was allowed, it took " + 
      str(np.round(((reaction_to_target_turning_red - targets_turned_red["TimeEntry"]).mean())).astype(int)) + 
      "ms on average to move forward")






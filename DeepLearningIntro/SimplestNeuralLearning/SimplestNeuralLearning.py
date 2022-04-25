
import numpy as np
np.random.seed(1) # Random generation will be the same each time

# Variables to store plot variables
# Allow to visualize error of the network in function of time
import matplotlib.pyplot as plt
xGraphCostFunction=[]
yGraphCostFunction=[]

# Neuron activation function: sigmoid
def sigmoid(x):
    return 1 / (1 + np.exp(-x))

# Derivative of the activation function
def sigmoidPrime(x):
    return x * (1 - x)


#===================================
#           Create data
#===================================

# Input data
inputs = np.array([ [0, 0, 0, 1],
                    [0, 0, 1, 1],
                    [0, 1, 1, 1],
                    [1, 0, 1, 0],
                    [1, 1, 1, 1]])

# Intended output
answers = np.array([[0],
                    [0],
                    [1],
                    [0],
                    [1]])

# Test data, to check the network is well trained
inputs_test = np.array([[1, 1, 1, 0],
                        [0, 1, 1, 0],
                        [0, 0, 1, 0],
                        [1, 0, 0, 0]])
#----------------------------------------


#------------------   Dimensions of neural network 
nb_input_neurons = 4
nb_hidden_neurons = 4
nb_output_neurons = 1
#------


# Random init for each neuron between -1 and 1
hidden_layer_weights = 2 * np.random.random((nb_input_neurons, nb_hidden_neurons)) - 1
output_layer_weights = 2 * np.random.random((nb_hidden_neurons, nb_output_neurons)) - 1

#===================================
#           Training Phase
#===================================

# Number of iteration for the training phase
nb_training_iteration = 10000

for i in range(nb_training_iteration):

    #---------------- FEED FORWARD -----------------
    # Broadcast information forward in neural network

    input_layer = inputs
    hidden_layer = sigmoid(np.dot(input_layer, hidden_layer_weights))   # Feedforward between input layer and hidden layer
    output_layer = sigmoid(np.dot(hidden_layer, output_layer_weights))  # Feedforward between hidden layer and ouput layer


    # ---------------- BACKPROPAGATION -----------------

    # Comput cost for each data. The goal is to dimish cost quickly.
    output_layer_error = (answers - output_layer)
    print("Error: " + str(output_layer_error))
    # output_layer_error = []

    # Compute a value with which we will correct weights between hidden and output layers
    output_layer_delta = output_layer_error * sigmoidPrime(output_layer)

    # Which are the weights between input and hidden layer that contributed to the cost, in which propotion?
    hidden_layer_error = np.dot(output_layer_delta, output_layer_weights.T)

    # Compute a value with which we will correct weights between input and hidden layers
    hidden_layer_delta = hidden_layer_error * sigmoidPrime(hidden_layer)


    # Correct weights
    output_layer_weights += np.dot(hidden_layer.T,output_layer_delta)
    hidden_layer_weights += np.dot(input_layer.T,hidden_layer_delta)

    # Display costs
    if (i % 10) == 0:
        cout = str(np.mean(np.abs(output_layer_error))) # Compute the mean of all errors' values
        print("Cost:" + cout)

        # Graph X -> Iterate through learning loop
        xGraphCostFunction.append(i)
        # Graph Y -> Cost value (3 numbers after comma)
        v = float("{0:.3f}".format(float(cout)))
        yGraphCostFunction.append(v)

#===================================
#           Test Phase
#===================================

# Use the trained network with the test data
input_layer = inputs_test
hidden_layer = sigmoid(np.dot(input_layer, hidden_layer_weights))
output_layer = sigmoid(np.dot(hidden_layer, output_layer_weights))

# Print result
print("------")
print("Result : ")
print(str(output_layer))

# Display graph
plt.plot(xGraphCostFunction, yGraphCostFunction)
plt.show()


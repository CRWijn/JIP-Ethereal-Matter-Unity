import numpy as np
import dtw
import matplotlib.pyplot as plt
import os

def show_matrix():
    path = "FMatrix.txt"
    
    with open(path, 'r') as file:
        lines = file.readlines()
        rows = len(lines)
        cols = len(lines[0].strip().split(' '))
        dims = (rows, cols)
        mat = np.zeros(dims)
        print(dims)
        for row in range(dims[0]):
            str_row = lines[row].strip().split(' ')
            for col in range(dims[1]):
                mat[row, col] = float(str_row[col])
        new_mat = mat
        np.set_printoptions(threshold=float('inf'))
        fig, ax = plt.subplots()
        #initials = new_mat[0, :]
        #diffs = np.diff(initials)
        #print(initials)
        #ax.plot(initials)
        #ax.plot(diffs)
        ax.matshow(new_mat, cmap=plt.cm.Blues)
        for i in range(dims[1]):
            for j in range(dims[0]):
                try:
                    c = round(new_mat[j, i])
                except:
                    c = new_mat[j, i]
                ax.text(i, j, str(c), va='center', ha='center', fontsize = 10)
        fig.show()
    

def plot_verif(save_path):
    path = "DUMP_RightKnee.txt"
    path2 = "DUMP_IANDJ.txt"
    live = []
    ref = []
    with open(path, 'r') as file:
        lines = file.readlines()
        live_chr = lines[0].strip().split(' ')
        ref_chr = lines[1].strip().split(' ')
        for x in range(len(live_chr)):
            live.append(float(live_chr[x]))

        for y in range(len(ref_chr)):
            ref.append(float(ref_chr[y]))
            
    x_ndx = []
    y_ndx = []
    with open(path2, 'r') as file:
        lines = file.readline()
        x_and_y_ndx_chr = lines.strip().replace('j', 'i').split('i:')
        
        for i in range(1, len(x_and_y_ndx_chr), 2):
            try:            
                x_ndx.append(int(x_and_y_ndx_chr[i])-1)
            except:
                pass
            try:
                y_ndx.append(int(x_and_y_ndx_chr[i+1])-1)
            except:
                pass
        #x_ndx = list(dict.fromkeys(x_ndx))
        #y_ndx = list(dict.fromkeys(y_ndx))
    
    #x = np.array(live)[x_ndx]
    #y = np.array(ref)[y_ndx]
    #fig, ax = plt.subplots()
    #ax.grid()
    #ax.plot(x)
    #ax.plot(y)
    ##ax.set_ylabel("Angle ($^\circ$C)")
    #ax.set_xlabel("Time Index")
    #ax.legend(['live','ref'])
    #fig.show()
    
    fig2, ax = plt.subplots()
    ax.plot(x_ndx, y_ndx)
    ax.set_title("Frame Matching")
    ax.set_xlabel("Live Frames")
    ax.set_ylabel("Ref Frames")
    plt.grid()
    fig2.set_size_inches(21, 10)
    plt.savefig(save_path + "frame_matching.png")
    #fig2.show()

    fig3, axs = plt.subplots(2, 1)
    axs[0].plot(live)
    axs[0].set_title("Live")
    axs[1].plot(ref)
    axs[1].set_title("Ref")
    #fig3.show()
    

def plot_avg():
    path = "../averages.txt"
    with open(path, 'r') as file:
        lines = file.readlines()
        x_and_y_chr = lines[0].strip().replace('y', 'x').split('x:')
        x = []
        y = []
        for i in range(0, len(x_and_y_chr[1:]), 2):
            try:            
                x.append(float(x_and_y_chr[i]))
            except:
                pass
            try:
                y.append(float(x_and_y_chr[i+1]))
            except:
                pass
        fig, ax = plt.subplots()
        ax.grid()
        ax.plot(x[::-1])
        ax.plot(y[::-1])
        ax.set_ylabel("Angle ($^\circ$C)")
        ax.set_xlabel("Time Index")
        ax.legend(['x','y'])
        fig.show()

def plot_comparison():
    path = "../data_dtw.txt"
    with open(path, 'r') as file:
        lines = file.readlines()
        x_chr = lines[0].split(', ')
        y_chr = lines[1].split(', ')
        x = []
        y = []
        x_and_y_ndx_chr = lines[2].strip().replace('j', 'i').split('i:')
        x_ndx = []
        y_ndx = []
        for i in range(len(x_chr)):
            try:
                x.append(float(x_chr[i]))
            except:
                pass
        for i in range(len(y_chr)):
            try:
                y.append(float(y_chr[i]))
            except:
                pass
        for i in range(0, len(x_and_y_ndx_chr), 2):
            try:            
                x_ndx.append(int(x_and_y_ndx_chr[i]))
            except:
                pass
            try:
                y_ndx.append(int(x_and_y_ndx_chr[i+1]))
            except:
                pass
    fig, axs = plt.subplots(2, 1)
    fig.tight_layout()
    axs[0].grid()
    axs[0].set_title("x")
    axs[0].plot(x)
    axs[0].set_ylabel("Angle ($^\circ$C)")
    axs[0].set_xlabel("Time Index")
    axs[1].grid()
    axs[1].set_title("y")
    axs[1].plot(y)
    axs[1].set_ylabel("Angle ($^\circ$C)")
    axs[1].set_xlabel("Time Index")
    fig.show()

    fig2, ax = plt.subplots()
    ax.plot(x_ndx, y_ndx)
    plt.grid()
    fig2.show()

def plot_ref():
    dir_path = "../ReferenceAngles/Squat/"
    paths = os.listdir(dir_path) #Get all the .txt files
    #paths = ["RightKnee.txt", "LeftKnee.txt", "RightAnkle.txt", "LeftAnkle.txt"] 
    for i in range(0, len(paths), 4): #Plot 2x2
        graphs_left = len(paths) - i
        if graphs_left == 1:
            fig, ax = plt.subplots()
            ax = np.array([ax])
        elif graphs_left == 2:
            fig, ax = plt.subplots(2, 1)
        else:
            fig, ax = plt.subplots(2, 2)
        ax = ax.flatten()

        fig.tight_layout()
        fig.supxlabel("Time Index")
        fig.supylabel("Angle ($^\circ$C)")
        for j in range(len(ax)):
            with open(dir_path + paths[i+j], 'r') as file:
                data_str = file.readline().replace(',', '.').split(" ")[:-1]
                data = []
                for datum in data_str:
                    data.append(float(datum))
            ax[j].plot(data)
            ax[j].set_ylim([0,180])
            ax[j].set_title(paths[i+j])
        fig.show()

def plot_report(plot_cost, save_path):
    dir_path = "Reporting/"

    # Ref and Live
    fig, axs = plt.subplots(2, 1)
    names = ["liveData", "refData"]
    ylim = [360, 0]
    for i, name in enumerate(names):
        with open(dir_path + name + ".txt", 'r') as file:
            data_str = file.readline().split(' ')
            data = []
            for datum in data_str:
                data.append(float(datum))
            axs[i].plot(data)
            axs[i].set_xlabel(name + " Time Index")
            axs[i].set_ylabel("Angle ($^\circ$)")
            axs[i].grid()
            ylim[0] = min(data + [ylim[0]])
            ylim[1] = max(data + [ylim[1]])
    for ax in axs:
        ax.set_ylim(ylim)
    fig.set_size_inches(21, 10)
    plt.savefig(save_path + "live_and_ref_matching.png")
    #fig.show()

    # Ref and Live Other Joint
    fig, axs = plt.subplots(2, 1)
    names = ["compareLiveData", "compareRefData"]
    ylim = [360, 0]
    lists = []
    for i, name in enumerate(names):
        with open(dir_path + name + ".txt", 'r') as file:
            data_str = file.readline().split(' ')
            data = []
            for datum in data_str:
                data.append(float(datum))
            lists.append(data)
            axs[i].plot(data)
            axs[i].set_xlabel(name + " Time Index")
            axs[i].set_ylabel("Angle ($^\circ$)")
            axs[i].grid()
            ylim[0] = min(data + [ylim[0]])
            ylim[1] = max(data + [ylim[1]])
    for ax in axs:
        ax.set_ylim(ylim)
    fig.set_size_inches(21, 10)
    plt.savefig(save_path + "live_and_ref_comparison.png", bbox_inches='tight')
    #fig.show()

    live = np.array(lists[0])
    ref = np.array(lists[1])

    # Other Knee Overlapped
    path = "DUMP_IANDJ.txt"
    x_ndx = []
    y_ndx = []
    with open(path, 'r') as file:
        lines = file.readline()
        x_and_y_ndx_chr = lines.strip().replace('j', 'i').split('i:')
        
        for i in range(1, len(x_and_y_ndx_chr), 2):
            try:            
                x_ndx.append(int(x_and_y_ndx_chr[i])-1)
            except:
                pass
            try:
                y_ndx.append(int(x_and_y_ndx_chr[i+1])-1)
            except:
                pass
    x = live[x_ndx]
    y = ref[y_ndx]
    fig, ax = plt.subplots()
    ax.grid()
    ax.plot(x)
    ax.plot(y)
    ax.set_ylabel("Angle ($^\circ$)")
    ax.set_xlabel("Time Index")
    ax.legend(['live','ref'])
    fig.set_size_inches(21, 10)
    plt.savefig(save_path + "comparison.png")
    #fig.show()

    # Live First Frame Distance
    fig, ax = plt.subplots()
    with open(dir_path + "frame0distances.txt", 'r') as file:
        data_str = file.readline().split(' ')
        data = []
        for datum in data_str:
            data.append(float(datum))
        ax.plot(data)
        ax.set_title("Live First Frame Distance To All Reference Frames")
        ax.set_xlabel("Reference Time Index")
        ax.set_ylabel("|live[0]-ref[time_index]|")
    fig.set_size_inches(21, 10)
    plt.savefig(save_path + "first_frame_dist.png")
    #fig.show()

    # Smoothed Distances
    fig, ax = plt.subplots()
    with open(dir_path + "smoothedDistances.txt", 'r') as file:
        data_str = file.readline().split(' ')
        data = []
        for datum in data_str:
            data.append(float(datum))
        ax.plot(data)
        ax.set_title("First Frames Distance Smoothed")
        ax.set_xlabel("Reference Time Index")
        ax.set_ylabel("Smoothed |live[0]-ref[time_index]|")
    data = np.array(data)
    with open(dir_path + "selectedIndices.txt", 'r') as file:
        data_str = file.readline().split(' ')
        ndx_data = []
        for datum in data_str:
            ndx_data.append(int(datum))
        ax.plot(ndx_data, data[ndx_data], 'ro')
    if (plot_cost):
        with open(dir_path + "selectedIndicesCosts.txt", 'r') as file:
            data_str = file.readline().split(' ')
            cost_data = []
            for datum in data_str:
                cost_data.append(float(datum))
            cost_data = np.round(cost_data)
            for i, cost in enumerate(cost_data):
                x = ndx_data[i] - 10
                y = data[x] + 2
                #plt.text(x, y, cost)
    ax.legend(["Data", "Detected Minimum"])
    fig.set_size_inches(21, 10)
    plt.savefig(save_path + "smoothed_and_minimums.png")
    #fig.show()

def plot_unsmoothed():
    dir_path = "Reporting/"
    # Live First Frame Distance
    fig, ax = plt.subplots()
    with open(dir_path + "frame0distances.txt", 'r') as file:
        data_str = file.readline().split(' ')
        data = []
        for datum in data_str:
            data.append(float(datum))
        ax.plot(data)
        ax.set_title("Unsmoothed Low Points Results")
        ax.set_xlabel("Reference Time Index")
        ax.set_ylabel("Unsmoothed |live[0]-ref[time_index]|")

        data = np.array(data)
        lowPoints = []
        oldDiff = -1
        for i in range(1, len(data)):
            newDiff = data[i] - data[i-1]
            if (oldDiff < 0 and newDiff >= 0):
                lowPoints.append(i)
            oldDiff = newDiff
        ax.plot(lowPoints, data[lowPoints], 'ro')
        ax.legend(["Data", "Minimums"])
    fig.show()
    

#plot_comparison()
#plot_avg()
#plot_ref()
#show_matrix()
save_path = "GraphsTemp/"
plot_verif(save_path)
plot_report(False, save_path)
#plot_unsmoothed()


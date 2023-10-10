import numpy as np
import dtw
import matplotlib.pyplot as plt
import os

def plot_verif():
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
        print(x_and_y_ndx_chr)
        
        for i in range(1, len(x_and_y_ndx_chr), 2):
            try:            
                x_ndx.append(int(x_and_y_ndx_chr[i])-1)
            except:
                pass
            try:
                y_ndx.append(int(x_and_y_ndx_chr[i+1])-1)
            except:
                pass
    
    x = np.array(live)[x_ndx]
    y = np.array(ref)[y_ndx]
    fig, ax = plt.subplots()
    ax.grid()
    ax.plot(x[::-1])
    ax.plot(y[::-1])
    #ax.plot(live)
    #ax.plot(ref)
    ax.set_ylabel("Angle ($^\circ$C)")
    ax.set_xlabel("Time Index")
    ax.legend(['live','ref'])
    fig.show()
    
    fig2, ax = plt.subplots()
    ax.plot(x_ndx, y_ndx)
    plt.grid()
    fig2.show()

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
            
    

#plot_comparison()
#plot_avg()
#plot_ref()
plot_verif()


import numpy as np
import dtw
import matplotlib.pyplot as plt

ref_points = np.linspace(0, 3*np.pi, num = 100)
ref = np.cos(ref_points)

query_points = np.linspace(0,3*np.pi, num=100) / 2
query = np.cos(query_points)

#Plot the query and reference
fig, axs = plt.subplots(2, 1)
axs[0].set_title("Reference")
axs[0].plot(ref)
axs[1].set_title("Query")
axs[1].plot(query)
plt.ylim(-1.5, 1.5)
fig.show()

fig2, axs = plt.subplots(2, 1)
alignment = dtw.dtw(query, ref)
axs[0].plot(alignment.index1, alignment.index2)
#axs[0].plot(alignment.index1, ref[alignment.index2])
axs[1].plot(alignment.index2, query[alignment.index1])
fig2.show()

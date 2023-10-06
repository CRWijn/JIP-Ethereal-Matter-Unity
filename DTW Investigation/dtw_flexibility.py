import numpy as np
import dtw
import matplotlib.pyplot as plt

ref_points = np.linspace(0, 2*np.pi, num = 100)
ref = np.cos(ref_points)

query_points = np.linspace(np.pi,2*np.pi+1, num=30)
query = np.cos(query_points)

#Plot the query and reference
fig, axs = plt.subplots(2, 1)
axs[0].set_title("Reference")
axs[0].plot(ref)
axs[1].set_title("Query")
axs[1].plot(query)
plt.ylim(-1, 1)
fig.show()

fig2, ax = plt.subplots()
alignment = dtw.dtw(query, ref)
ax.plot(alignment.index1, alignment.index2)
#ax.plot(alignment.index2, query[alignment.index1])
fig2.show()
